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

    async getData(fromDate, toDate, ids) {
        const result = await api.createECReport(fromDate, toDate, ids);
        if (result.code === 0) {
            this.data = result.data;
        } else {
            this.data = [];
            viewUtils.showError(result.message, false);
        }
    }
    show() {
        alert("Привет!");
    }
}

const ecReportDetailedPage = new ECReportDetailedPage();

document.addEventListener('DOMContentLoaded', ecReportDetailedPage.show.bind(ecReportDetailedPage));
//document.onclick = ecReportDetailedPage.onPageClick.bind(ecReportDetailedPage);
//document.addEventListener('DOMContentLoaded', gatesPage.show.bind(gatesPage));