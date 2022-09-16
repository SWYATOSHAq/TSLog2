/*
    Функционал для доступа к другим программным интерфейсам по http протоколу.
    Автор - Буров Михаил.
    Создано - 2022.09.08.
*/

// Класс для доступа к внешним программным интерфейсам по протоколу http
class Api {
    // 
    async getWarehouseFillingReportData1() {
        const url = "./api/v1/Reports/WarehouseFillingReport1";
        let response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    async getWarehouseFillingReportData2(fromDate, toDate) {
        const url = "./api/v1/Reports/WarehouseFillingReport2" +
            "?from_date=" + fromDate +
            "&to_date=" + toDate;
        let response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }
}

const api = new Api();