/*
    Страница по детализированному отчету трудозатрат из Босс Контроля по конктретному клиенту за выбранный период.
    Автор - Аввакумов Алексей (avvakumov@tslog.ru).
*/

class ECReportDetailedPage {

    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    async getData(fromDate, toDate, id) {
        const result = await api.createECReportDetailed(fromDate, toDate, id);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            this.data = [];
            viewUtils.showError(result.message, false);
        }
    }

    clearReport() {
        const reportBody = document.getElementById("detailed-report-tbody");
        reportBody.replaceChildren();
        this.data = [];
    }

    showReportTitle() {
        const reportHeaderB = document.getElementById("report-header-b")
        reportHeaderB.textContent = "Детализированный отчёт по трудозатратам персонала (БоссКонтроль)" +
            " с " + utils.getUrlParameter("from_date") +
            " по " + utils.getUrlParameter("to_date") +
            " (ВКЛЮЧАЯ!)";
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"),
            utils.getUrlParameter("to_date"),
            utils.getUrlParameter("partners_id"));
        this.showReport();
        this.showReportTitle();
    }

    exportToExcel() {
        const reportTable = document.getElementById("detailed-report-table");

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

    showReport() {
        const reportBody = document.getElementById("detailed-report-tbody");
        for (let element of this.data) {

            const tr = document.createElement("tr");
            viewUtils.createCell(tr, ["table-cell"], [element.partnerName]);
            viewUtils.createCell(tr, ["table-cell"], [element.workerFullName]);
            viewUtils.createCell(tr, ["table-cell"], [element.positionName]);
            let date = new Date(element.startWork);
            const startDate = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString();
            viewUtils.createCell(tr, ["table-cell"], [startDate]);
            date = new Date(element.finishWork);
            const finishDate = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString();
            viewUtils.createCell(tr, ["table-cell"], [finishDate]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.duration / 60)]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.bcCost)]);
            viewUtils.createCell(tr, ["table-cell"], [element.siteName]);

            reportBody.append(tr);
        }        
    }
    show() {
        this.createReport();
    }
}

const ecReportDetailedPage = new ECReportDetailedPage();

document.addEventListener('DOMContentLoaded', ecReportDetailedPage.show.bind(ecReportDetailedPage));
