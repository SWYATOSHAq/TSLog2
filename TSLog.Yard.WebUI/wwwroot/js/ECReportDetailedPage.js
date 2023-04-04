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

    //createParameters() {
    //    let result = "";
    //    result = utils.addParameter(result, "from_date", "from-date-input");
    //    result = utils.addParameter(result, "to_date", "to-date-input");
    //    const id = this.getIds(",")
    //    result = utils.addParameterValue(result, "partners_id", id);
    //    return result != "" ? "?" + result : "";
    //}

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
        //const reportFooter = document.getElementById("detailed-report-tfoot");
        reportBody.replaceChildren();
        //reportFooter.replaceChildren();
        this.data = [];
    }

    async createReport() {
        this.clearReport();
        await this.getData(utils.getUrlParameter("from_date"),
            utils.getUrlParameter("to_date"),
            utils.getUrlParameter("partners_id"));
        this.showReport();
        //this.showReportTitle();

        //const partnerIdTh = document.getElementById("partner-id-th");
        //partnerIdTh.click();
    }

    showReport() {
        const reportBody = document.getElementById("detailed-report-tbody");
        //const reportFooter = document.getElementById("detailedreport-tfoot");
        for (let element of this.data) {

            const tr = document.createElement("tr");
            viewUtils.createCell(tr, ["table-cell"], [element.partnerName]);
            //viewUtils.createCell(tr, ["table-cell"], [utils.getUrlParameter("from_date") + " - " + utils.getUrlParameter("to_date")]);
            viewUtils.createCell(tr, ["table-cell"], [element.workerFullName]);
            viewUtils.createCell(tr, ["table-cell"], [element.positionName]);
            //viewUtils.createCell(tr, ["table-cell"], [element.startWork]);
            let date = new Date(element.startWork);
            const startDate = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString();
            viewUtils.createCell(tr, ["table-cell"], [startDate]);
            date = new Date(element.finishWork);
            const finishDate = date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + " - " + date.toLocaleDateString();
            viewUtils.createCell(tr, ["table-cell"], [finishDate]);
            viewUtils.createCell(tr, ["table-cell"], [utils.displayFloat(element.duration / 60)]);
            viewUtils.createCell(tr, ["table-cell"], [element.partnerId]);
            viewUtils.createCell(tr, ["table-cell"], [element.siteName]);

            reportBody.append(tr);
        }        
    }
    show() {
        alert("Привет!");
        this.createReport();
    }
}

const ecReportDetailedPage = new ECReportDetailedPage();

document.addEventListener('DOMContentLoaded', ecReportDetailedPage.show.bind(ecReportDetailedPage));
//document.onclick = ecReportDetailedPage.onPageClick.bind(ecReportDetailedPage);
//document.addEventListener('DOMContentLoaded', gatesPage.show.bind(gatesPage));