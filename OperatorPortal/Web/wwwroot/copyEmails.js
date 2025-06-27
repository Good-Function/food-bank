if (!window.__emailCopyListenerAdded) {
    window.__emailCopyListenerAdded = true;

    document.body.addEventListener("emailCopyDone", function(evt) {
        const { count, total } = evt.detail;
        const emails = document.getElementById('email-buffer').value;
        const missing = document.getElementById('email-missing-teczka').value;

        navigator.clipboard.writeText(emails)
            .then(() => alert(`Skopiowano do schowka ${count} z ${total} maili.\nTeczki bez kontaktu email: ${missing}`))
            .catch(() => alert('Kopiowanie nie udane. Spróbuj ponownie.'));
    });
}