const container = document.getElementById('event-container');

// ---------- Helpers ----------

// Builds a DOM element. Text is always set with textContent, so user input is never parsed as HTML (prevents XSS).
function el(tag, { text, className, attrs } = {}, children = []) {
    const node = document.createElement(tag);
    if (text !== undefined) node.textContent = text;
    if (className) node.className = className;
    for (const [key, value] of Object.entries(attrs || {})) node.setAttribute(key, value);
    children.forEach(child => node.appendChild(child));
    return node;
}

function field(label, value) {
    return el('div', {}, [el('strong', { text: `${label}: ` }), document.createTextNode(value)]);
}

function toDateInput(isoDate) {
    return isoDate.split('T')[0];
}

// Returns the error message from an API response: plain text or ASP.NET validation details
async function readError(res) {
    const text = await res.text();
    try {
        const data = JSON.parse(text);
        if (data.errors) return Object.values(data.errors).flat().join('\n');
        return data.title || text;
    } catch {
        return text || `Request failed (${res.status})`;
    }
}

function readEventForm(prefix) {
    const id = name => document.getElementById(prefix ? prefix + name[0].toUpperCase() + name.slice(1) : name);
    return {
        name: id('name').value,
        startDate: id('startDate').value,
        endDate: id('endDate').value,
        maxRegistrations: parseInt(id('maxRegistrations').value, 10),
        location: id('location').value
    };
}

// ---------- Events list ----------

async function loadEvents() {
    container.replaceChildren();
    try {
        const res = await fetch(`${API_BASE_URL}/schedule`);
        if (!res.ok) throw new Error(await readError(res));
        const events = await res.json();

        if (events.length === 0) {
            container.appendChild(el('li', { text: 'No events yet. Create the first one above.' }));
            return;
        }
        events.forEach(ev => container.appendChild(renderEvent(ev)));
    } catch (err) {
        console.error(err);
        container.appendChild(el('li', { text: 'Failed to load events. Make sure the API server is running.' }));
    }
}

function renderEvent(ev) {
    const panel = el('div', { className: 'event-panel' });

    const mapLink = el('a', {
        text: '📍 View on map',
        className: 'map-link',
        attrs: { href: ev.googleMapsLink, target: '_blank', rel: 'noopener noreferrer' }
    });

    const button = (text, className, onClick) => {
        const b = el('button', { text, className, attrs: { type: 'button' } });
        b.addEventListener('click', onClick);
        return b;
    };

    const actions = el('div', { className: 'actions' }, [
        button('Register', 'register-btn', () => showRegistrationForm(ev, panel)),
        button('Participants', 'participants-btn', () => showParticipants(ev, panel)),
        button('Weather', 'weather-btn', () => showWeather(ev, panel)),
        button('Edit', 'edit-btn', () => editEvent(ev.id)),
        button('Delete', 'delete-btn', () => deleteEvent(ev.id))
    ]);

    return el('li', {}, [
        el('h3', { text: ev.name }),
        field('Dates', `${toDateInput(ev.startDate)} → ${toDateInput(ev.endDate)}`),
        field('Location', ev.location),
        field('Registrations', `${ev.registeredCount} / ${ev.maxRegistrations}`),
        mapLink,
        actions,
        panel
    ]);
}

// ---------- Registration, participants, weather ----------

function showRegistrationForm(ev, panel) {
    const nameInput = el('input', { attrs: { type: 'text', placeholder: 'Full name', required: '', maxlength: '50' } });
    const dobInput = el('input', { attrs: { type: 'date', required: '' } });
    const form = el('form', { className: 'inline-form' }, [
        el('label', { text: 'Name' }), nameInput,
        el('label', { text: 'Date of birth' }), dobInput,
        el('button', { text: 'Confirm registration', attrs: { type: 'submit' } })
    ]);

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        try {
            const res = await fetch(`${API_BASE_URL}/${ev.id}/registration`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: nameInput.value, dateOfBirth: dobInput.value })
            });
            if (res.ok) {
                alert('Registered successfully!');
                loadEvents();
            } else {
                alert('Registration failed.\n' + await readError(res));
            }
        } catch (err) {
            console.error(err);
            alert('Error connecting to server.');
        }
    });

    panel.replaceChildren(form);
}

async function showParticipants(ev, panel) {
    panel.replaceChildren(el('p', { text: 'Loading participants...' }));
    try {
        const res = await fetch(`${API_BASE_URL}/${ev.id}/registration`);
        if (!res.ok) throw new Error(await readError(res));
        const users = await res.json();

        if (users.length === 0) {
            panel.replaceChildren(el('p', { text: 'No participants yet.' }));
            return;
        }
        const list = el('ol', { className: 'participants' },
            users.map(u => el('li', { text: `${u.name} (born ${u.dateOfBirth})` })));
        panel.replaceChildren(el('strong', { text: 'Participants:' }), list);
    } catch (err) {
        console.error(err);
        panel.replaceChildren(el('p', { text: 'Failed to load participants.' }));
    }
}

async function showWeather(ev, panel) {
    panel.replaceChildren(el('p', { text: 'Loading weather...' }));
    try {
        const res = await fetch(`${API_BASE_URL}/${ev.id}/weather`);
        if (!res.ok) throw new Error(await readError(res));
        const w = await res.json();

        const lines = [el('strong', { text: `Weather in ${w.location} on ${w.date}:` }), el('div', { text: w.summary })];
        if (w.forecastAvailable) {
            lines.push(el('div', { text: `🌡️ ${w.minTemperatureC}°C – ${w.maxTemperatureC}°C` }));
            if (w.precipitationProbability !== null) {
                lines.push(el('div', { text: `🌧️ Chance of rain: ${w.precipitationProbability}%` }));
            }
        }
        panel.replaceChildren(el('div', { className: 'weather' }, lines));
    } catch (err) {
        console.error(err);
        panel.replaceChildren(el('p', { text: 'Weather is currently unavailable.' }));
    }
}

// ---------- Create, edit, delete ----------

async function deleteEvent(id) {
    if (!confirm('Are you sure you want to delete this event?')) return;

    try {
        const res = await fetch(`${API_BASE_URL}/${id}`, { method: 'DELETE' });
        if (res.ok) {
            alert('Event deleted.');
            loadEvents();
        } else {
            alert('Failed to delete event.\n' + await readError(res));
        }
    } catch (err) {
        console.error(err);
        alert('Error deleting event.');
    }
}

async function editEvent(id) {
    try {
        const res = await fetch(`${API_BASE_URL}/${id}`);
        if (!res.ok) throw new Error('Event not found');

        const ev = await res.json();
        document.getElementById('editId').value = ev.id;
        document.getElementById('editName').value = ev.name;
        document.getElementById('editStartDate').value = toDateInput(ev.startDate);
        document.getElementById('editEndDate').value = toDateInput(ev.endDate);
        document.getElementById('editMaxRegistrations').value = ev.maxRegistrations;
        document.getElementById('editLocation').value = ev.location;

        const section = document.getElementById('edit-event');
        section.hidden = false;
        section.scrollIntoView({ behavior: 'smooth' });
    } catch (err) {
        console.error(err);
        alert('Failed to load event.');
    }
}

function cancelEdit() {
    document.getElementById('edit-form').reset();
    document.getElementById('edit-event').hidden = true;
}

document.getElementById('event-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    try {
        const res = await fetch(API_BASE_URL, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(readEventForm(''))
        });

        if (res.ok) {
            alert('Event created!');
            e.target.reset();
            loadEvents();
        } else {
            alert('Failed to create event.\n' + await readError(res));
        }
    } catch (err) {
        console.error(err);
        alert('Error connecting to server.');
    }
});

document.getElementById('edit-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('editId').value;
    try {
        const res = await fetch(`${API_BASE_URL}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(readEventForm('edit'))
        });

        if (res.ok) {
            alert('Event updated!');
            cancelEdit();
            loadEvents();
        } else {
            alert('Failed to update event.\n' + await readError(res));
        }
    } catch (err) {
        console.error(err);
        alert('Error connecting to server.');
    }
});

document.getElementById('cancel-edit').addEventListener('click', cancelEdit);

loadEvents();
