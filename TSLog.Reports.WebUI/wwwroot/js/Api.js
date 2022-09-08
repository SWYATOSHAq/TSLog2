/*
    Функционал для доступа к другим программным интерфейсам по http протоколу.
    Автор - Буров Михаил.
    Создано - 2022.09.08.
*/

// Класс для доступа к внешним программным интерфейсам по протоколу http
class Api {
    // 
    async getWarehouseFillingReportData() {
        const url = "./api/v1/Reports/WarehouseFillingReport";
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