/*
    Функционал для доступа к другим программным интерфейсам по http протоколу.
    Автор - Буров Михаил.
*/

// Класс для доступа к внешним программным интерфейсам по протоколу http
class Api {
    // Вход работника на участок работы
    async enterSite(workerId, siteId, partnerId, note) {
        const url = "./api/v1/BossControl/EnterSite";
        const data = {};
        data.workerId = workerId;
        data.siteId = siteId;
        data.partnerId = partnerId;
        data.note = note;
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ workerId: workerId, })
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("worker", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Выход работника с участка работы
    async leaveSite(workerId) {
        const url = "./api/v1/BossControl/LeaveSite";
        const data = {};
        data.workerId = workerId;
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("worker", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Получение всех занятых работников
    async getBusyWorkers() {
        const url = "./api/v1/BossControl/BusyWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("sitesWorkers", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Получение всех свободных работников
    async getFreeWorkers() {
        const url = "./api/v1/BossControl/FreeWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("workers", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Получение свободных работников, которые находятся в текущей смене
    async getFreeShiftWorkers() {
        const url = "./api/v1/BossControl/FreeShiftWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("sitesWorkers", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирование отчёта о трудозатратах за смену
    async createPositionsShiftReport(date, shift) {
        let url = "./api/v1/BossControl/PositionsShiftReport" +
            "?date=" + date +
            "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }
}

const api = new Api();