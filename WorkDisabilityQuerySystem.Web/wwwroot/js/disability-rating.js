const form = document.querySelector("#rating-form");
const affectedFunction = document.querySelector("#affected-function");

for (const select of form.querySelectorAll("[data-rating-select]")) {
    const field = $(select);
    field.select2({
        width: "100%",
        placeholder: select.dataset.placeholder,
        allowClear: true,
        maximumInputLength: 100,
        language: {
            searching: () => "搜尋中…",
            noResults: () => "查無符合的項目",
            errorLoading: () => "搜尋失敗，請稍後重試。",
            inputTooLong: () => "關鍵字最多 100 個字元。"
        },
        ajax: {
            url: select.dataset.url,
            dataType: "json",
            delay: 250,
            data: (params) => ({ term: params.term ?? "" }),
            processResults: (items) => ({
                results: items.map((item) => ({
                    id: item.id,
                    text: item.label,
                    detail: item.detail
                }))
            })
        },
        templateResult: (item) => {
            if (!item.detail) return item.text;
            const result = $("<span>").text(item.text);
            $("<small>").addClass("d-block text-body-secondary").text(item.detail).appendTo(result);
            return result;
        },
        templateSelection: (item) => item.detail ? item.text + " · " + item.detail : item.text
    });

    if (select.id === "ImpairmentCode") {
        field.on("change", () => {
            affectedFunction.hidden = select.value !== "13.07.08.00";
            if (affectedFunction.hidden) affectedFunction.querySelector("select").value = "";
        });
    }
}
const resultPanel = document.querySelector("#rating-result");
const errorMessage = form.querySelector("[data-rating-error]");
const submitButton = form.querySelector('button[type="submit"]');

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    if (submitButton.disabled) return;

    resultPanel.hidden = true;
    errorMessage.textContent = "";
    for (const message of form.querySelectorAll("[data-valmsg-for]")) {
        message.textContent = "";
    }

    const inputs = new FormData(form);
    submitButton.disabled = true;
    try {
        const response = await fetch(form.action, {
            method: "POST",
            body: inputs,
            headers: { Accept: "application/json" }
        });
        if (response.redirected) {
            location.assign(response.url);
            return;
        }

        const result = await response.json();
        if (!response.ok) {
            let displayed = false;
            for (const message of form.querySelectorAll("[data-valmsg-for]")) {
                const errors = result.errors?.[message.dataset.valmsgFor];
                if (errors?.length) {
                    message.textContent = errors.join(" ");
                    displayed = true;
                }
            }
            errorMessage.textContent = result.errors?.[""]?.join(" ") ??
                (displayed ? "" : "查詢失敗，請稍後重試。");
            return;
        }

        const values = {
            pd: result.pd + "%",
            wpi: inputs.get("Wpi") + "%",
            fecLabel: "FEC 調整（第 " + result.fecRank + " 級）",
            fec: result.fecAdjusted + "%",
            occupationLabel: "職業調整（群組 " + result.groupNumber + " · " + result.variant + "）",
            occupation: result.occupationAdjusted + "%",
            ageLabel: "年齡調整（受傷時 " + inputs.get("Age") + " 歲）"
        };
        for (const output of resultPanel.querySelectorAll("[data-result]")) {
            output.textContent = values[output.dataset.result];
        }
        resultPanel.hidden = false;
        resultPanel.focus();
    } catch {
        errorMessage.textContent = "查詢失敗，請稍後重試。";
    } finally {
        submitButton.disabled = false;
    }
});


