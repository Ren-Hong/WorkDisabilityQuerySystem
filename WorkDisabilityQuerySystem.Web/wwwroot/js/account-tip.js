const accountDetails = document.querySelector(".wdqs-account-details");

if (accountDetails) {
    document.addEventListener("click", event => {
        if (!accountDetails.contains(event.target)) {
            accountDetails.open = false;
        }
    });

    accountDetails.addEventListener("keydown", event => {
        if (event.key === "Escape" && accountDetails.open) {
            accountDetails.open = false;
            accountDetails.querySelector("summary").focus();
        }
    });
}
