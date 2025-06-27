module Organizations.Templates.MailingList

open Oxpecker.ViewEngine

let View (mails:string) =
    Fragment() {
        input(type'="text", value=mails, id="email-buffer", hidden="true")
        raw """
<script>
  (function() {
    var el = document.getElementById('email-buffer');
    navigator.clipboard.writeText(el.value)
      .then(() => alert('Skopiowano do schowka.'))
      .catch(() => alert('Kopiowanie nie udane. Spróbuj ponownie.'));
  })();
</script>
"""
    }
