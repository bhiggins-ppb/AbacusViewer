$( document ).ready(function() {
    $('#search-events').click(function () {
        this.href = this.href + '?eventId=' + document.getElementById('event-id-input').value;
    });

    $('#reload-event').click(function () {
        window.location.reload();
    });
});

const validation = {
    isNumber: function (str) {
        const pattern = /^\d+$/;
        return pattern.test(str);  // returns a boolean
    }
};

function validateEventId() {
    const eventId = document.getElementById('event-id-input').value;
    const formDomNode = document.getElementById('search-form');
    const isValidEventId = validation.isNumber(eventId);
    if (isValidEventId) {
        formDomNode.classList.add('has-success');
        formDomNode.classList.remove('has-error');
    } else {
        formDomNode.classList.add('has-error');
        formDomNode.classList.remove('has-success');
    }
    return isValidEventId;
}
