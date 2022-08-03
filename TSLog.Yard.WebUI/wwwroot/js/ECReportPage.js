/*
    Страница по приходам/расходам (БоссКоннтроль + Биллинг).
    2022.07.06 - добавлен выбор НЕСКОЛЬКИХ клиентов.
    Автор - Буров Михаил.
*/

// Страница отчёта по приходам/расходам (БоссКоннтроль + Биллинг) - 
// отчёта по затратам по БоссКонтролю и по выручке из Биллинга по клиентам.
class ECReportPage {
    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        result = utils.addParameter(result, "from_date", "from-date-input");
        result = utils.addParameter(result, "to_date", "to-date-input");
        const ids = this.getIds(",")
        result = utils.addParameterValue(result, "partners_ids", ids);
        return result != "" ? "?" + result : "";
    }

    getIds(separator = " ") {
        const partnersUl = document.getElementById("partners-ul");
        const items = partnersUl.children;
        let result = "";
        for (let i = 0; i < items.length; i++) {
            const li = items[i];
            const partnerId = li.getAttribute("partnerId");
            const checkbox = document.getElementById("partner-" + partnerId + "-input");
            if (checkbox.checked) {
                if (result.length > 0) {
                    result += separator;
                }
                result += partnerId;
            }
        }
        return result;
    }

    getSelectedClientsCount() {
        const partnersUl = document.getElementById("partners-ul");
        const items = partnersUl.children;
        let result = 0;
        for (let i = 0; i < items.length; i++) {
            const li = items[i];
            const partnerId = li.getAttribute("partnerId");
            const checkbox = document.getElementById("partner-" + partnerId + "-input");
            if (checkbox.checked) {                
                result++;
            }
        }
        return result;
    }

    async getData(fromDate, toDate, ids) {        
        const result = await api.createECReport(fromDate, toDate, ids);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            this.data = [];
            viewUtils.showError(result.message, false);
        }
    }

    clearReport() {
        const reportBody = document.getElementById("report-tbody");
        const reportFooter = document.getElementById("report-tfoot");
        reportBody.replaceChildren();
        reportFooter.replaceChildren();
        this.data = [];
    }

    showReport() {
        let totalBcDuration = 0.0;
        let totalBcCost = 0.0;
        let totalBillingTotalIncome = 0.0;
        let totalBillingStoreIncome = 0.0;
        let totalBillingConsumablesIncome = 0.0;
        let totalBillingHandlingIncome = 0.0;
        let totalHandlingProfit = 0.0;
        let totalTotalProfit = 0.0;
        let avgHandlingMargin = 0.0;
        let avgTotalMargin = 0.0;

        let downtime = 0.0;
        let downcost = 0.0;
        const reportBody = document.getElementById("report-tbody");
        const reportFooter = document.getElementById("report-tfoot");
        for (let element of this.data) {
            if (element.partnerId === "DOWN") {
                downtime = element.bcDuration;
                downcost = element.bcCost;
                continue;
            }

            const tr = document.createElement("tr");
            //viewUtils.createCell(tr, ["table-cell"], [element.partnerId]);
            //viewUtils.createCell(tr, ["table-cell"], [utils.getUrlParameter("from_date") + " - " + utils.getUrlParameter("to_date")]);
            viewUtils.createCell(tr, ["table-cell"], [element.partnerName]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.bcDuration)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.bcCost)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.billingTotalIncome)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.billingStoreIncome)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.billingConsumablesIncome)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.billingHandlingIncome)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.handlingProfit)]);            
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.totalProfit)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.handlingMargin)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.totalMargin)]);
            reportBody.append(tr);

            totalBcDuration += element.bcDuration;
            totalBcCost += element.bcCost;
            totalBillingTotalIncome += element.billingTotalIncome;
            totalBillingStoreIncome += element.billingStoreIncome;
            totalBillingConsumablesIncome += element.billingConsumablesIncome;
            totalBillingHandlingIncome += element.billingHandlingIncome;            
        }
        totalHandlingProfit = totalBillingHandlingIncome - totalBcCost;
        totalTotalProfit = totalBillingTotalIncome - totalBcCost - downcost; // Простои учтены ТОЛЬКО в общеё прибыли и в общей марже, в обработке не учтены.
        avgHandlingMargin = totalBillingHandlingIncome != 0.0 ? totalHandlingProfit / totalBillingHandlingIncome * 100.0 : 0.0;
        avgTotalMargin = totalBillingTotalIncome != 0.0 ? totalTotalProfit / totalBillingTotalIncome * 100.0 : 0.0;

        const boldStyleTitle = "font-weight: bold; width: 12%";
        const boldStyleContent = "font-weight: bold; width: 8.8%";

        // Выводим итог
        let tr = document.createElement("tr");
        //viewUtils.createCell(tr, ["table-cell"], [element.partnerId]);
        //viewUtils.createCell(tr, ["table-cell"], [utils.getUrlParameter("from_date") + " - " + utils.getUrlParameter("to_date")], boldStyle);
        viewUtils.createCell(tr, ["table-cell"], ["Итого:"], boldStyleTitle);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalBcDuration)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalBcCost)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalBillingTotalIncome)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalBillingStoreIncome)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalBillingConsumablesIncome)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalBillingHandlingIncome)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalHandlingProfit)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(totalTotalProfit)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(avgHandlingMargin)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(avgTotalMargin)], boldStyleContent);
        reportFooter.append(tr);

        // Выводим простои
        tr = document.createElement("tr");
        viewUtils.createCell(tr, ["table-cell"], ["Простои:"], boldStyleTitle);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(downtime)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(downcost)], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        viewUtils.createCell(tr, ["table-cell"], [""], boldStyleContent);
        reportFooter.append(tr);
    }

    showReportTitle() {
        const reportHeaderB = document.getElementById("report-header-b")
        reportHeaderB.textContent = "Отчёт по затратам (БоссКонтроль) и выручке (Биллинг)" +
            " с " + utils.getUrlParameter("from_date") +
            " по " + utils.getUrlParameter("to_date") +
            " (ВКЛЮЧАЯ!)";
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"),
            utils.getUrlParameter("to_date"),
            utils.getUrlParameter("partners_ids"));
        this.showReport();
        this.showReportTitle(); 

        const partnerIdTh = document.getElementById("partner-id-th");
        partnerIdTh.click();
    }

    exportToExcel() {
        const reportTable = document.getElementById("report-table");

        let fromDate = utils.getUrlParameter("from_date");
        if (fromDate == "")
            fromDate = utils.getNowDate();
        let toDate = utils.getUrlParameter("to_date");
        if (toDate == "")
            toDate = utils.getNowDate();
        const fileName = "ecReport_" + fromDate + "_" + toDate;
        const sheetName = fromDate + " - " + toDate;

        viewUtils.tableToExcel_v2(reportTable, fileName, "xlsx", sheetName);
    }

    print() {
        window.print();
    }

    showHideDescription() {
        const showHideDescriptionButton = document.getElementById("show-hide-description-button");
        const descriptionDiv = document.getElementById("description-div");
        if (descriptionDiv.classList.contains("d-none")) {
            showHideDescriptionButton.textContent = "Порядок рассчёта и комментарии к отчёту (скрыть)";
            showHideDescriptionButton.title = "Скрыть порядок рассчёта и комментарии к отчёту";
            descriptionDiv.classList.remove("d-none");
        } else {
            showHideDescriptionButton.textContent = "Порядок рассчёта и комментарии к отчёту (показать)";
            showHideDescriptionButton.title = "Показать порядок рассчёта и комментарии к отчёту";
            descriptionDiv.classList.add("d-none");
        }
    }

    onChangeDate() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    onPartnersSpanClick(event) {        
        const partnersDiv = document.getElementById('partners-div');
        const partnersSpan = document.getElementById("partners-span");        
        const partnersUl = document.getElementById("partners-ul");
        if (partnersDiv.classList.contains('visible')) {
            partnersDiv.classList.remove('visible');
            partnersSpan.style.cssText = "padding: 0; text-align: center; width: 240px; border-radius: 4px";
            partnersUl.style.cssText = "";
        }
        else {            
            partnersDiv.classList.add('visible');
            partnersSpan.style.cssText = "padding: 0; text-align: center; width: 240px; border-radius: 4px 4px 0 0";
            partnersUl.style.cssText = "border-radius: 0 0 4px 4px";
        }
        event.stopPropagation();
    }

    onPageClick() {
        const partnersDiv = document.getElementById('partners-div');
        partnersDiv.classList.remove('visible');
    }

    onPartnerInputChange() {
        const partnersSpan = document.getElementById("partners-span");
        const selectedClientsCount = this.getSelectedClientsCount();
        partnersSpan.textContent = selectedClientsCount > 0 ? "Клиентов выбрано: " + selectedClientsCount.toString() : "Выберите клиентов";

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }

    onPartnerInputClick(event) {
        event.stopPropagation();
    }

    selectPartners(partnersIds) {
        const idsArray = partnersIds.split(",");
        const ids = new Set(idsArray);

        const partnersUl = document.getElementById("partners-ul");
        const items = partnersUl.children;
        let selectedClientsCount = 0;
        for (let i = 0; i < items.length; i++) {
            const li = items[i];
            const partnerId = li.getAttribute("partnerId");
            const checkbox = document.getElementById("partner-" + partnerId + "-input");
            checkbox.checked = ids.has(partnerId);
            selectedClientsCount += ids.has(partnerId) ? 1 : 0;
        }

        const partnersSpan = document.getElementById("partners-span");
        partnersSpan.textContent = selectedClientsCount > 0 ? "Клиентов выбрано: " + selectedClientsCount.toString() : "Выберите клиентов";
    }

    async show() {        
        const fromDatePicker = $("#from-date-input");
        const toDatePicker = $("#to-date-input");
        fromDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });
        toDatePicker.datepicker({
            format: "dd.mm.yyyy",
            autoclose: true,
            language: "ru",
            todayBtn: true,
            todayHighlight: true,
        });

        let fromDate = utils.getUrlParameter("from_date");
        let toDate = utils.getUrlParameter("to_date");
        if (fromDate == "") {
            fromDate = utils.getNowDate();
        }
        fromDatePicker.datepicker("setDate", fromDate);
        fromDatePicker.on("changeDate", this.onChangeDate.bind(this));
        if (toDate == "") {
            toDate = fromDate;
        }
        toDatePicker.datepicker("setDate", toDate);
        toDatePicker.on("changeDate", this.onChangeDate.bind(this));

        const partnersIds = utils.getUrlParameter("partners_ids");
        this.selectPartners(partnersIds);

        const partnersSpan = document.getElementById("partners-span");
        partnersSpan.onclick = this.onPartnersSpanClick.bind(this);

        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        viewUtils.sortTable();
        await this.createReport();
    }
}

const ecReportPage = new ECReportPage();

document.addEventListener('DOMContentLoaded', ecReportPage.show.bind(ecReportPage));
document.onclick = ecReportPage.onPageClick.bind(ecReportPage);