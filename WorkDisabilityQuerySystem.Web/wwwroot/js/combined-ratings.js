const combineForm = document.querySelector("#combine-ratings");
const combineResult = document.querySelector("#combine-result");
const combineError = document.querySelector("#combine-error");

combineForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const submit = combineForm.querySelector('button[type="submit"]');
    if (submit.disabled) return;

    submit.disabled = true;
    combineResult.hidden = true;
    combineError.hidden = true;
    try {
        const response = await fetch(combineForm.action, {
            method: "POST",
            body: new FormData(combineForm)
        });
        if (response.redirected) {
            location.assign(response.url);
            return;
        }
        const result = await response.json();
        if (!response.ok) throw new Error(result.message ?? "合併失敗，請重試。");

        document.querySelector("#combined-percent").textContent = result.combined + "%";
        const steps = result.steps.map((step) => {
            const item = document.createElement("li");
            item.textContent = step.current + "% 與 " + step.next + "% 合併 → " + step.combined + "%";
            return item;
        });
        document.querySelector("#combined-steps").replaceChildren(...steps);
        combineResult.hidden = false;
        combineResult.focus();
    } catch (error) {
        combineError.textContent = error instanceof SyntaxError ? "合併失敗，請重試。" : error.message;
        combineError.hidden = false;
    } finally {
        submit.disabled = false;
    }
});
