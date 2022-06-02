/*
    Страница по расходам по БоссКоннтролю.
    Автор - Буров Михаил.
*/

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
        //result = utils.addParameter(result, "partner_id", "partners-select");
        return result != "" ? "?" + result : "";
    }

    async getData(fromDate, toDate) {
        const result = await api.createECReport(fromDate, toDate);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            viewUtils.showError(result.message);
        }
    }

    clearReport() {
        const elementsTbody = document.getElementById("report-tbody");
        elementsTbody.replaceChildren();
    }

    showReport() {
        const h3 = document.getElementById("report-header")
        h3.textContent = "Отчёт по зтратам на персонал из БоссКонтроля" +
            " с " + utils.getUrlParameter("from_date") +
            " по " + utils.getUrlParameter("to_date");

        let totalBcDuration = 0.0;
        let totalBcCost = 0.0;
        let totalBillingIncome = 0.0;
        let totalProfit = 0.0;
        let avgMargin = 0.0;

        const tbody = document.getElementById("report-tbody");
        for (let element of this.data) {
            const tr = document.createElement("tr");
            //viewUtils.createCell(tr, ["table-cell"], [element.partnerId]);
            viewUtils.createCell(tr, ["table-cell"], [utils.getUrlParameter("from_date") + " - " + utils.getUrlParameter("to_date")]);
            viewUtils.createCell(tr, ["table-cell"], [element.partnerName]);
            viewUtils.createCell(tr, ["table-cell"], [parseFloat(element.bcDuration).toFixed(2)]);
            viewUtils.createCell(tr, ["table-cell"], [parseFloat(element.bcCost).toFixed(2)]);
            viewUtils.createCell(tr, ["table-cell"], [parseFloat(element.billingIncome).toFixed(2)]);
            viewUtils.createCell(tr, ["table-cell"], [""]);
            viewUtils.createCell(tr, ["table-cell"], [""]);
            viewUtils.createCell(tr, ["table-cell"], [""]);            
            viewUtils.createCell(tr, ["table-cell"], [parseFloat(element.profit).toFixed(2)]);
            viewUtils.createCell(tr, ["table-cell"], [""]);
            viewUtils.createCell(tr, ["table-cell"], [parseFloat(element.margin).toFixed(2)]);
            tbody.append(tr);

            totalBcDuration += element.bcDuration;
            totalBcCost += element.bcCost;
            totalBillingIncome += element.billingIncome;
        }

        totalProfit = totalBillingIncome - totalBcCost;
        avgMargin = totalBillingIncome != 0.0 ? totalProfit / totalBillingIncome * 100.0 : 0.0;
        const tr = document.createElement("tr");
        //viewUtils.createCell(tr, ["table-cell"], [element.partnerId]);
        viewUtils.createCell(tr, ["table-cell"], [utils.getUrlParameter("from_date") + " - " + utils.getUrlParameter("to_date")], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], ["Итого:"], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [parseFloat(totalBcDuration).toFixed(2)], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [parseFloat(totalBcCost).toFixed(2)], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [parseFloat(totalBillingIncome).toFixed(2)], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [""], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [""], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [""], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [parseFloat(totalProfit).toFixed(2)], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [""], "font-weight: bold");
        viewUtils.createCell(tr, ["table-cell"], [parseFloat(avgMargin).toFixed(2)], "font-weight: bold");
        tbody.append(tr);
    }

    onChangeParameters() {        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        this.showShiftInfo();
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"), utils.getUrlParameter("to_date"));
        this.showReport()
    }

    onChangeDate() {
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
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
        
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
        await this.createReport();
    }
}

const ecReportPage = new ECReportPage();

document.addEventListener('DOMContentLoaded', ecReportPage.show.bind(ecReportPage));