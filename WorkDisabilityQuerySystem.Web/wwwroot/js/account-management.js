(() => {
    const grid = document.querySelector("#account-management");
    const filter = document.querySelector("#account-filter");
    const list = document.querySelector("#account-list");
    const pages = document.querySelector("#account-pages");
    const pagination = document.querySelector("#account-pagination");
    const status = document.querySelector("#account-list-status");
    const create = document.querySelector("#create-account-dialog");
    const edit = document.querySelector("#edit-account-dialog");
    const remove = document.querySelector("#delete-account-dialog");
    const csrf = create.querySelector('input[name="__RequestVerificationToken"]').value;
    const pageSize = 10;
    let accounts = [];
    let page = 1;
    let keyword = "";
    let level = "";

    async function request(url, options) {
        const response = await fetch(url, options);
        if (response.redirected || response.status === 401 || response.status === 403) {
            throw new Error("登入已過期或權限不足，請重新登入。");
        }
        const data = await response.json();
        if (!response.ok) throw new Error(data.message ?? "操作失敗，請稍後重試。");
        return data;
    }

    function showStatus(message, isError = false) {
        status.hidden = !message;
        status.textContent = message;
        status.classList.toggle("text-danger", isError);
    }

    function openDialog(dialog, account) {
        const form = dialog.querySelector("form");
        form.reset();
        dialog.querySelector("[data-dialog-status]").textContent = "";
        if (account) {
            form.elements.AccountId.value = account.accountId;
            dialog.querySelector("[data-account-name]").textContent = account.username;
            if (dialog === edit) {
                form.elements.Level.value = account.level;
                form.elements.DisplayName.value = account.displayName ?? "";
            }
        }
        dialog.showModal();
    }

    function button(text, className, onClick) {
        const element = document.createElement("button");
        element.type = "button";
        element.className = className;
        element.textContent = text;
        element.addEventListener("click", onClick);
        return element;
    }

    function renderRow(account, index) {
        const row = document.createElement("tr");
        for (const value of [index + 1, account.username, account.displayName || "—"]) {
            row.insertCell().textContent = value;
        }
        const role = document.createElement("span");
        role.className = "account-role";
        role.textContent = account.level === grid.dataset.adminLevel ? "管理員" : "一般使用者";
        row.insertCell().append(role);

        const actions = row.insertCell();
        actions.className = "text-end text-nowrap";
        if (account.accountId === grid.dataset.currentId) {
            actions.textContent = "目前帳號";
        } else {
            actions.append(
                button("編輯", "btn btn-sm account-edit me-2", () => openDialog(edit, account)),
                button("刪除", "btn btn-sm account-delete", () => openDialog(remove, account))
            );
        }
        return row;
    }

    function render() {
        const filtered = accounts.filter((account) =>
            (!level || account.level === level) &&
            [account.username, account.displayName].some((value) => value?.toLowerCase().includes(keyword)));
        const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
        page = Math.min(page, totalPages);
        const offset = (page - 1) * pageSize;
        list.replaceChildren(...filtered.slice(offset, offset + pageSize).map((account, index) => renderRow(account, offset + index)));
        if (!filtered.length) {
            const row = list.insertRow();
            const cell = row.insertCell();
            cell.colSpan = 5;
            cell.className = "text-center py-4";
            cell.textContent = "查無符合的帳號";
        }
        pagination.hidden = totalPages <= 1;
        pages.replaceChildren();

        function addPage(label, target, disabled, current = false) {
            const control = button(label, "btn btn-sm " + (current ? "btn-primary" : "wdqs-outline"), () => {
                page = target;
                render();
                pages.querySelector('[aria-current="page"]')?.focus();
            });
            control.disabled = disabled;
            if (current) control.setAttribute("aria-current", "page");
            if (label === "‹") control.setAttribute("aria-label", "上一頁");
            if (label === "›") control.setAttribute("aria-label", "下一頁");
            pages.append(control);
        }

        if (totalPages > 1) {
            addPage("‹", page - 1, page === 1);
            for (let number = Math.max(1, page - 2); number <= Math.min(totalPages, page + 2); number++) {
                addPage(String(number), number, false, number === page);
            }
            addPage("›", page + 1, page === totalPages);
        }
    }

    async function loadAccounts() {
        try {
            accounts = await request(grid.dataset.listUrl);
            render();
            showStatus("");
        } catch (error) {
            showStatus(error.message, true);
        }
    }

    filter.addEventListener("submit", (event) => {
        event.preventDefault();
        keyword = filter.querySelector("#account-keyword").value.trim().toLowerCase();
        level = filter.querySelector("#account-level-filter").value;
        page = 1;
        render();
    });
    filter.addEventListener("reset", () => {
        keyword = level = "";
        page = 1;
        render();
    });
    document.querySelector("#open-create-account").addEventListener("click", () => openDialog(create));

    for (const dialog of [create, edit, remove]) {
        const form = dialog.querySelector("form");
        const feedback = dialog.querySelector("[data-dialog-status]");
        dialog.querySelector("[data-close-dialog]").addEventListener("click", () => dialog.close());
        dialog.addEventListener("close", () => form.reset());
        form.addEventListener("submit", async (event) => {
            event.preventDefault();
            const submit = form.querySelector('[type="submit"]');
            if (submit.disabled) return;
            submit.disabled = true;
            feedback.textContent = "";
            try {
                if (dialog === remove) {
                    await request(grid.dataset.listUrl + "/" + encodeURIComponent(form.elements.AccountId.value), {
                        method: "DELETE", headers: { RequestVerificationToken: csrf }
                    });
                } else {
                    const body = new FormData(form);
                    body.set("__RequestVerificationToken", csrf);
                    await request(dialog === create ? form.action : grid.dataset.profileUrl, { method: "POST", body });
                }
                dialog.close();
                await loadAccounts();
            } catch (error) {
                feedback.textContent = error.message;
                feedback.classList.add("text-danger");
            } finally {
                submit.disabled = false;
            }
        });
    }

    showStatus("載入中…");
    loadAccounts();
})();
