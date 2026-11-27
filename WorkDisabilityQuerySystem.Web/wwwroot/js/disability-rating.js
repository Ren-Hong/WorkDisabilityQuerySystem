const form = document.querySelector("#rating-form");
const affectedFunction = document.querySelector("#affected-function");

const categoryLabels = {
    "03": "心血管－心臟與主動脈",
    "04": "心血管－全身與肺動脈",
    "05": "呼吸系統",
    "06": "消化系統",
    "07": "泌尿與生殖系統",
    "08": "皮膚",
    "09": "造血系統",
    "10": "內分泌系統",
    "11": "耳鼻喉",
    "12": "視覺系統",
    "13": "中樞與周邊神經系統",
    "14": "精神與行為障礙",
    "15": "脊椎",
    "16": "上肢",
    "17": "下肢",
    "18": "疼痛"
};

// Each array starts at code XX.01 and follows the code's second segment.
const subcategoryLabels = {
    "03": ["心臟瓣膜", "冠狀動脈", "先天性心臟病", "心肌病變", "心包膜", "心律不整"],
    "04": ["高血壓性心血管疾病", "主動脈", "周邊血管", "肺循環"],
    "05": ["氣喘", "呼吸障礙", "癌症"],
    "06": ["上消化道", "大腸、直腸與肛門", "腸皮瘻管", "肝膽", "疝氣"],
    "07": ["上泌尿道", "尿路改道", "膀胱", "尿道", "生殖系統"],
    "08": ["外觀毀損", "疤痕與植皮", "接觸性皮膚炎", "乳膠過敏", "皮膚癌"],
    "09": ["造血功能損傷"],
    "10": ["糖尿病"],
    "11": ["耳", "臉部與外觀", "鼻、喉及相關構造"],
    "12": ["視力", "視野", "視覺系統"],
    "13": [
        "意識障礙", "發作性神經障礙", "警醒障礙", "認知損傷", "語言障礙", "行為與情緒障礙",
        "腦神經", "姿勢、步態與動作", "上肢", "脊髓障礙", "慢性疼痛", "周邊神經"
    ],
    "14": ["精神與行為障礙"],
    "15": ["頸椎", "胸椎", "腰椎", "皮質脊髓束", "骨盆"],
    "16": ["手臂", "肩", "手肘／前臂", "手腕", "手／多指", "手指"],
    "17": ["腿部", "骨盆", "髖", "股骨", "膝", "脛骨／腓骨", "踝", "足", "腳趾"]
};

// California Permanent Disability Rating Schedule, section 3, parts B and C.
const occupationCategories = [
    "", "專業／技術／文書", "手部操作密集", "機械操作", "清潔／服務照護",
    "駕駛", "勞務／物料搬運", "維修／安裝", "營建", "其他"
];
const strengthLevels = ["", "很輕", "輕", "中等", "重", "很重"];
const occupationGroups = {
    110: ["專業職務", "使用部分鍵盤，需站立及走動。"],
    111: ["專業與文書", "大量使用鍵盤，也需要站立及走動。"],
    112: ["久坐文書", "長時間坐姿，鍵盤使用需求高。"],
    120: ["精細技術", "多坐姿，需靈巧使用手工具。"],
    210: ["溝通型專業", "說話及聽辨需求高，可能需開車前往工作地點。"],
    211: ["一般文書", "頻繁用手指操作或拿取物件，部分工作使用鍵盤。"],
    212: ["室內專業與醫療", "多在室內工作，可能外出，手部操作較少。"],
    213: ["巡檢與外勤專業", "室內外皆有工作，偶爾攀爬或走過不平地面。"],
    214: ["活動型文書／教學／零售", "頻繁溝通、觀察及操作物件，走動需求較高。"],
    220: ["高精密作業", "醫療、電子或光學等精密工作，視力與手部操作需求高。"],
    221: ["輕型裝配與備料", "重複使用手指和手工具，重視視力。"],
    230: ["輕型機械操作", "操作或照管機械，主要需求在手部。"],
    240: ["輕型現場服務", "以接待、看顧等服務為主，手臂操作需求低。"],
    250: ["大眾運輸與輕型配送", "在道路上操作車輛，視覺與聽覺需求較高。"],
    251: ["頻繁開車的外勤", "外勤業務或檢查工作，需頻繁開車往返工作地點。"],
    290: ["個人服務與美容", "重視視覺和外觀，手臂使用需求較高。"],
    310: ["醫療技術", "重視視聽及溝通，中等體力組內身體多數部位的負荷較低。"],
    311: ["醫療處置", "執行治療時脊椎負荷較高，也重視頭部相關功能。"],
    320: ["精密裝配與維修", "使用手工具做精密作業，上肢需求較高。"],
    321: ["一般裝配", "使用手工具，精密度要求低於 320。"],
    322: ["餐食準備與服務", "包括備餐與服務，精密操作要求低於 320、321。"],
    330: ["沖壓與鋸切機械", "操作沖壓、鋸切等設備，上肢負荷在同系列較高。"],
    331: ["機械監控與加工", "觀察、控制及調整機械，偶爾彎腰。"],
    332: ["大型固定設備監控", "在廠房等場所監看設備，偶爾做機械調整。"],
    340: ["一般清潔", "清理建物或設備，使用清潔工具，偶爾搬抬或攀爬。"],
    341: ["高處清潔與服務", "常在高處工作，脊椎及下肢需求較高。"],
    350: ["道路重型車駕駛", "在道路上開重型車，可能兼做裝載、固定貨物等工作。"],
    351: ["工地與場內重型設備操作", "操作起重機、堆高機等設備，脊椎及下肢需求較高。"],
    360: ["一般搬運與包裝", "需要較多搬抬及走動，偶爾短距攀爬。"],
    370: ["機械裝配、安裝與維修", "維修車輛或設備，需要技術和相當的體力。"],
    380: ["技術營建", "施工時常需攀爬，四肢和脊椎負荷較高。"],
    390: ["保全與體育指導", "需要大量走動，有時走過不平地面或攀爬。"],
    420: ["肉品加工與重型手作", "上肢施力需求高，下肢需求在重體力組中較低。"],
    430: ["機械輔助金屬成形", "使用重型工具或以上肢施力，也需搬抬物件。"],
    460: ["重物搬運與機台裝卸", "經常抬搬重物，脊椎與下肢負荷高。"],
    470: ["重型安裝與維修", "安裝或修理時全身的體力需求高。"],
    480: ["工地重勞務與助手", "重度搬抬或施力，以脊椎及上肢負荷為主。"],
    481: ["需攀爬的技術營建", "在不同高度施工，攀爬頻繁，上肢負荷高。"],
    482: ["高處高風險營建與維護", "在高處施工或維護，需維持平衡，上肢施力需求很高。"],
    490: ["警察與消防", "可能在突發、危險的情況下執行高負荷任務。"],
    491: ["農牧與園藝", "照料土地或動物，涉及搬抬及其他體力工作。"],
    492: ["林業與漁業", "高負荷戶外作業，常需平衡、攀爬或在崎嶇地面工作。"],
    493: ["運動與體能指導", "需要運動表現，整體體能要求低於 590。"],
    560: ["超重物搬運與高施力", "搬動大型或很重的物件，或需使出很大的力量。"],
    590: ["高強度競技與特技", "需要專項訓練、技巧及高強度全身體能。"]
};

const picker = document.querySelector("#rating-picker");
const pickerModal = new bootstrap.Modal(picker);
let pickerActive = false;
const pickerTitle = picker.querySelector("#picker-title");
const pickerPath = picker.querySelector("#picker-path");
const pickerStatus = picker.querySelector("#picker-status");
const pickerChoices = picker.querySelector("#picker-choices");
const pickerResults = picker.querySelector("#picker-results");
const pickerSearch = picker.querySelector("#picker-search");
const pickerCount = picker.querySelector("#picker-count");
const pickerItems = picker.querySelector("#picker-items");
const pickerBack = picker.querySelector("#picker-back");

picker.addEventListener("show.bs.modal", () => { pickerActive = true; });
picker.addEventListener("shown.bs.modal", () => pickerTitle.focus());
picker.addEventListener("hide.bs.modal", () => { pickerActive = false; });

function addPickerChoice(label, choose, detail) {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "list-group-item list-group-item-action";
    button.textContent = label;
    if (detail) {
        const description = document.createElement("small");
        description.className = "d-block text-body-secondary";
        description.textContent = detail;
        button.append(description);
    }
    button.addEventListener("click", choose);
    pickerChoices.append(button);
}

function addPickerGroups(items, keyFor, labelFor, choose, describe) {
    const counts = new Map();
    for (const item of items) {
        const key = keyFor(item);
        counts.set(key, (counts.get(key) ?? 0) + 1);
    }
    for (const key of [...counts.keys()].sort()) {
        const label = [key, labelFor(key)].filter(Boolean).join(" ");
        addPickerChoice(label + "（" + counts.get(key) + "）", () => choose(key), describe?.(key));
    }
}

function setupPicker({ field, trigger, summary, categoryName, groupName, resultName, placeholder,
    categoryLabel, groupLabel, describeGroup, mapItem, noun }) {
    let catalog;

    trigger.addEventListener("click", () => {
        let category = "";
        let group = "";
        let items = [];

        function showStep(step, label) {
            pickerTitle.textContent = step + "/3　選擇" + label;
            pickerPath.textContent = [
                category && (categoryLabel(category) || category),
                group && (groupLabel(group) || group)
            ].filter(Boolean).join(" › ");
            pickerPath.hidden = !pickerPath.textContent;
            pickerStatus.textContent = "";
            pickerChoices.replaceChildren();
            pickerChoices.hidden = step === 3;
            pickerResults.hidden = step !== 3;
            pickerBack.hidden = step === 1;
            pickerChoices.scrollTop = 0;
            pickerResults.scrollTop = 0;
            pickerTitle.focus();
        }

        function showCategories() {
            category = group = "";
            showStep(1, categoryName);
            addPickerChoice("直接搜尋全部" + noun, () => showResults(true));
            addPickerGroups(items, item => item.category, categoryLabel, showGroups);
        }

        function showGroups(code) {
            category = code;
            group = "";
            showStep(2, groupName);
            pickerBack.onclick = showCategories;
            const matching = items.filter(item => item.category === category);
            addPickerChoice("查看這類全部項目（" + matching.length + "）", () => showResults(false));
            addPickerGroups(matching, item => item.group, groupLabel, key => {
                group = key;
                showResults(false);
            }, describeGroup);
        }

        function commit(item) {
            field.value = item.id;
            summary.textContent = item.text;
            trigger.textContent = "變更" + noun;
            form.querySelector('[data-valmsg-for="' + field.name + '"]').textContent = "";
            field.dispatchEvent(new Event("change"));
            pickerModal.hide();
        }

        function updateResults() {
            const terms = pickerSearch.value.trim().toLowerCase().split(/\s+/);
            const matching = items.filter(item => (!category || item.category === category) &&
                (!group || item.group === group) && terms.every(term => item.searchText.includes(term)));

            const rows = document.createDocumentFragment();
            for (const item of matching) {
                const row = document.createElement("button");
                row.type = "button";
                row.className = "list-group-item list-group-item-action";
                row.textContent = item.text;
                row.addEventListener("click", () => commit(item));
                rows.append(row);
            }
            pickerItems.replaceChildren(rows);
            pickerItems.scrollTop = 0;
            pickerCount.textContent = matching.length
                ? matching.length + " 個項目"
                : "查無符合的項目，可縮短關鍵字或返回上一層。";
        }

        function showResults(directSearch) {
            showStep(3, resultName);
            pickerBack.onclick = category ? () => showGroups(category) : showCategories;
            pickerSearch.placeholder = placeholder;
            pickerItems.setAttribute("aria-label", resultName);
            pickerSearch.value = "";
            updateResults();
            if (directSearch) pickerSearch.focus();
        }

        async function load() {
            showStep(1, categoryName);
            pickerStatus.textContent = "正在載入" + noun + "項目…";
            try {
                if (!catalog) {
                    const response = await fetch(field.dataset.url, { headers: { Accept: "application/json" } });
                    if (response.redirected) {
                        location.assign(response.url);
                        return;
                    }
                    if (!response.ok) throw new Error("Lookup failed");
                    catalog = (await response.json()).map(mapItem);
                }
                items = catalog;
                if (pickerActive && pickerSearch.oninput === updateResults) showCategories();
            } catch {
                if (pickerActive && pickerSearch.oninput === updateResults) {
                    pickerStatus.textContent = "無法載入" + noun + "項目。";
                    addPickerChoice("重新載入", load);
                }
            }
        }

        pickerSearch.oninput = updateResults;
        picker.addEventListener("hidden.bs.modal", () => trigger.focus(), { once: true });
        pickerModal.show(trigger);
        load();
    });
}

const impairment = document.querySelector("#ImpairmentCode");
impairment.addEventListener("change", () => {
    affectedFunction.hidden = impairment.value !== "13.07.08.00";
    if (affectedFunction.hidden) affectedFunction.querySelector("select").value = "";
});

setupPicker({
    field: impairment,
    trigger: document.querySelector("#choose-impairment"),
    summary: document.querySelector("#impairment-summary"),
    categoryName: "部位／系統",
    groupName: "細分類",
    resultName: "診斷／失能項目",
    placeholder: "搜尋英文名稱或代碼",
    categoryLabel: code => categoryLabels[code],
    groupLabel: code => subcategoryLabels[code.slice(0, 2)]?.[Number(code.slice(3)) - 1],
    mapItem: item => ({
        id: item.id,
        category: item.id.slice(0, 2),
        group: item.id.slice(0, 5),
        text: item.id + " · " + item.label,
        searchText: (item.id + " " + item.label).toLowerCase()
    }),
    noun: "診斷"
});

setupPicker({
    field: document.querySelector("#OccupationId"),
    trigger: document.querySelector("#choose-occupation"),
    summary: document.querySelector("#occupation-summary"),
    categoryName: "工作大類",
    groupName: "職業群組",
    resultName: "職稱與行業",
    placeholder: "搜尋職稱、行業或群組碼",
    categoryLabel: code => occupationCategories[Number(code)],
    groupLabel: code => occupationGroups[code]?.[0],
    describeGroup: code => occupationGroups[code]
        ? "體力需求：" + strengthLevels[Number(code[0])] + "。 " + occupationGroups[code][1]
        : "",
    mapItem: item => {
        const group = String(item.groupNumber);
        const text = [item.label, item.detail, "群組 " + group].filter(Boolean).join(" · ");
        return { id: item.id, category: group[1], group, text, searchText: text.toLowerCase() };
    },
    noun: "職業"
});

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


