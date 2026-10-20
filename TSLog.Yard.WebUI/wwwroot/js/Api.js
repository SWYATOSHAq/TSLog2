/*
    Функционал для доступа к другим программным интерфейсам по http протоколу.
    Автор - Буров Михаил (m.burov@tslog.ru).
*/

// Класс для доступа к внешним программным интерфейсам по протоколу http
class Api {

    // Возвращает результат HTTP запроса
    async getHttpResult(fieldName, url, options = null) {
        // TODO: check fieldName not empty!
        const response = options != null ?
            await fetch(url, options) :
            await fetch(url);

        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject(fieldName, await response.json());
        }
        else {
            //return utils.createErrorObject(await response.json());
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает признак пользователя партнёра (клиента)
    async getIsPartnerUser() {
        const url = "./api/v1/yard/IsPartnerUser";
        return await this.getHttpResult("isPartnerUser", url);
    }

    // Возвращает плановую продолжительность операции двора типу операции и виду автомашины (стандартное значение для ПОЛНОЙ машины)
    async getStandardPlanOperationDuration(operationTypeCode, vehicleKindCode) {
        const url = "./api/v1/yard/StandardPlanOperationDuration" + "?operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode;
        return await this.getHttpResult("planDuration", url);
    }

    // Возвращает плановую продолжительность операции с учётом назначенных заказов (расходов)/приходов
    async getFillingPlanOperationDuration(operationTypeCode, vehicleKindCode, warehouseActions) {
        const data = {
            operationTypeCode: operationTypeCode,
            vehicleKindCode: vehicleKindCode,
            warehouseActions: warehouseActions
        };
        const url = "./api/v1/yard/FillingPlanOperationDuration";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("planDuration", url, options);
    }

    // Возвращает плановую продолжительность операции с учётом назначенных заказов (алгоритм Башлыкова)
    async getHoodManPlanOperationDuration(operationTypeCode, vehicleKindCode, outgoes) {
        const data = {
            operationTypeCode: operationTypeCode,
            vehicleKindCode: vehicleKindCode,
            outgoes: outgoes
        };
        const url = "./api/v1/yard/HoodManPlanOperationDuration";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("planDuration", url, options);
    }

    // Возвращает плановую продолжительность загрузки заказа (на отгрузку)
    async getHoodManPlanOutgoDuration(outgoId, isPallet, partnerId) {
        const url = "./api/v1/yard/HoodManPlanOutgoDuration" + "?outgo_id=" + outgoId + "&is_pallet=" + isPallet + "&partner_id=" + partnerId;
        return await this.getHttpResult("outgoDuration", url);
    }

    // Возвращает плановые трудозатраты операции двора типу операции и виду автомашины 
    // (+ плановые трудозатраты только линейного персонала), стадартное значение плановых трудозатрат
    async getStandartPlanOperationLaborCost(operationTypeCode, vehicleKindCode) {
        const url = "./api/v1/yard/StandartPlanOperationLaborCost?" + "operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode;
        return await this.getHttpResult("data", url);
    }

    // Возвращает плановые трудозатраты операции двора типу операции и виду автомашины (+ плановые трудозатраты только линейного персонала)
    // с учётом назначенных заказов
    async getFillingPlanOperationLaborCost(operationTypeCode, vehicleKindCode, warehouseActions) {
        const data = {
            operationTypeCode: operationTypeCode,
            vehicleKindCode: vehicleKindCode,
            warehouseActions: warehouseActions
        };
        const url = "./api/v1/yard/FillingPlanOperationLaborCost";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("data", url, options);
    }

    // Возвращает плановые трудозатраты операции двора типу операции и виду автомашины (+ плановые трудозатраты только линейного персонала)
    // с учётом назначенных заказов (расширенный алгоритм Башлыкова)
    async getHoodManPlanOperationLaborCost(operationTypeCode, vehicleKindCode, partnerId, outgoes) {
        const data = {
            operationTypeCode: operationTypeCode,
            vehicleKindCode: vehicleKindCode,
            partnerId: partnerId,
            outgoes: outgoes
        };
        const url = "./api/v1/yard/HoodManPlanOperationLaborCost";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("laborCost", url, options);
    }

    // Возвращает допустимые интервалы по дате, типу операции двора, виду автомашины
    async getAvailableIntervals(date, operationTypeCode, vehicleKindCode) {
        const url = "./api/v1/yard/AvailableIntervals?" + "date=" + date + "&operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode;
        return await this.getHttpResult("availableIntervals", url);
    }

    // Возвращает свободные или зарезервированные (НЕ ЗАНЯТЫЕ) ворота по дате, типу операции двора, виду автомашины.
    async getAvailableGates(startDate, finishDate, operationTypeCode, vehicleKindCode, operationId) {
        const url = "./api/v1/yard/AvailableGates?" + "operation_type=" + operationTypeCode + "&vehicle_kind=" + vehicleKindCode + "&start_date=" + startDate + "&finish_date=" + finishDate + "&operation_id="
            + operationId;
        return await this.getHttpResult("availableGates", url);
    }

    // Возвращает клиентского менеджера (основного)
    async getPartnerManager(partnerId) {
        const url = "./api/v1/yard/PartnerManager" + "?partner_id=" + partnerId;
        return await this.getHttpResult("manager", url);
    }

    // Возвращает операцию двора по идентификатору
    async getOperation(id) {
        const url = "./api/v1/yard/Operation?id=" + id;
        return await this.getHttpResult("operation", url);
    }

    // Возвращает массив операций двора по заданным параметрам поиска
    async getOperations(parameters) {
        const url = "./api/v1/yard/Operations" + parameters;
        return await this.getHttpResult("operations", url);
    }

    // Возвращает активные (машины на территории складского комплекса) операции двора
    async getActiveOperations() {
        const url = "./api/v1/yard/ActiveOperations";
        return await this.getHttpResult("operations", url);
    }

    // Возвращает массив операций двора для охраны (которые должны приехать или могут уехать)
    async getGuardOperations() {
        const url = "./api/v1/yard/GuardOperations";
        return await this.getHttpResult("operations", url);
    }

    // Создайт операцию двора и возвращает созданную (для сотрудника ТСЛ) без подтверждения
    async createOperation(data) {
        const url = "./api/v1/yard/CreateOperation";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Создайт операцию двора и возвращает созданную (для сотрудника ТСЛ) с подтверждением
    async createConfirmOperation(data) {
        const url = "./api/v1/yard/CreateConfirmOperation";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Создайт операцию двора и возвращает созданную (для сотрудника партнёра)
    async createOperationByPartner(data) {
        const url = "./api/v1/yard/CreateOperationByPartner";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Правит операцию двора и возвращает исправленную (для сотрудника ТСЛ)
    async editOperation(data) {
        const url = "./api/v1/yard/EditOperation";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Правит операцию двора и возвращает исправленную (для сотрудника партнёра)
    async editOperationByPartner(data) {
        const url = "./api/v1/yard/EditOperationByPartner";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Правит примечание  операции двора
    async editOperationNote(id, note) {
        const data = {
            id: id,
            note: note
        };
        const url = "./api/v1/yard/SetOperationNote";
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Стартует операцию двора (автомашина заезжает на территорию складского комплекса)
    async startOperation(id) {
        const url = "./api/v1/yard/StartOperation";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // // Возвращает массив работников, которые могут быть назначены на данную должность, по шаблону поиска
    async getPositionWorkers(positionCode, pattern) {
        const url = "./api/v1/yard/PositionWorkers?" + "name_pattern=" + pattern + "&position_code=" + positionCode;
        return await this.getHttpResult("data", url, options);
    }

    // Возвращает массов линейных сотрудников (всех)
    async getLinearWorkers() {
        const url = "./api/v1/yard/LinearWorkers";
        return await this.getHttpResult("data", url);
    }

    // Возвращает массов линейных сотрудников (только свободных)
    async getLinearFreeWorkers() {
        const url = "./api/v1/yard/LinearFreeWorkers";
        return await this.getHttpResult("data", url);
    }

    // Возвращает массов всех работников
    async getAllWorkers() {
        const url = "./api/v1/yard/AllWorkers";
        return await this.getHttpResult("data", url);
    }

    // Возвращает массив работников, назначенных на операцию двора
    async getOperationWorkers(operationId) {
        const url = "./api/v1/yard/OperationWorkers?operation_id=" + operationId;
        return await this.getHttpResult("data", url);
    }

    // Назначает работника на операцию двора
    async addOperationWorker(operationId, workerId, positionCode) {
        const url = "./api/v1/yard/AddOperationWorker";
        const data = {
            operationId: operationId,
            workerId: workerId,
            positionCode: positionCode,
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("data", url, options);
    }

    // Снимает назначенного работника с операции двора
    async removeOperationWorker(operationId, workerId, positionCode) {
        const url = "./api/v1/yard/RemoveOperationWorker";
        const data = {
            operationId: operationId,
            workerId: workerId,
            positionCode: positionCode,
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("data", url, options);
    }

    // Возвращает массив промежутков работ сотрудников из систем БоссКонтролль
    async getBossControlWorks(operationId) {
        const url = "./api/v1/yard/BossControlWorks?" + "operation_id=" + operationId;
        return await this.getHttpResult("works", url);
    }

    // Начинает работы по операции двора (ставит машину на ворота)
    async startOperationWork(id) {
        const url = "./api/v1/yard/StartOperationWork";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Заканчивает работы по операции двора (снимает машину с ворот)
    async finishOperationWork(id) {
        const url = "./api/v1/yard/FinishOperationWork";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Завершает операцию двора (выпускает машину за территорию)
    async finishOperation(id) {
        const url = "./api/v1/yard/FinishOperation";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Отменяет операуию двора
    async cancelOperation(id) {
        const url = "./api/v1/yard/CancelOperation";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Удаляет операию двора
    async deleteOperation(id) {
        const url = "./api/v1/yard/DeleteOperation";
        const data = {
            id: id
        }
        const options = {
            method: "DELETE",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("operation", url, options);
    }

    // Формирует оперативный отчёт (за смену)
    async createOperationalReport(date, shift) {
        const url = "./api/v1/yard/OperationalReport?date=" + date + "&shift=" + shift;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт диспетчера (за период)
    async createDispatcherReport(fromDate, toDate) {
        const url = "./api/v1/yard/DispatcherReport?from_date=" + fromDate + "&to_date=" + toDate;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт по трудозатратам
    async createLaborReport(fromDate, toDate) {
        const url = "./api/v1/yard/LaborReport?from_date=" + fromDate + "&to_date=" + toDate;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт по производительности за смену
    async createShiftPerfomanceReport(date, shift) {
        const url = "./api/v1/yard/ShiftPerfomanceReport?date=" + date + "&shift=" + shift;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт по трудозатратам за смену 2
    async createShiftPerfomanceReport2(date, shift) {
        const url = "./api/v1/yard/ShiftPerfomanceReport?date=" + date + "&shift=" + shift;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт по трудозатратам за смену 3
    async createGateManagement(date) {
        const url = "./api/v1/yard/GateManagement?date=" + date;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт по выручке/расходам (БоссКоннтроль + Биллинг)
    async createECReport(fromDate, toDate, ids) {
        const url = "./api/v1/yard/ECReport?from_date=" + fromDate + "&to_date=" + toDate + "&partners_ids=" + ids;
        return await this.getHttpResult("data", url);
    }

    // Формирует детализированный отчёт по трудозатратам персонала на конктретном клиенте (манданте) за выбранный период
    async createECReportDetailed(fromDate, toDate, id) {
        const url = "./api/v1/yard/ECReportDetailed?from_date=" + fromDate + "&to_date=" + toDate + "&partners_id=" + id;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт производительности по работникам (за период), первая версия
    async createWorkersPerfomanceReport(fromDate, toDate) {
        const url = "./api/v1/yard/WorkersPerfomanceReport?from_date=" + fromDate + "&to_date=" + toDate;
        return await this.getHttpResult("data", url);
    }

    // Формирует отчёт производительности по работникам (за период), вторая версия
    async createWorkersPerfomanceReport2(fromDate, toDate) {
        const url = "./api/v1/yard/WorkersPerfomanceReport2?from_date=" + fromDate + "&to_date=" + toDate;
        return await this.getHttpResult("data", url);
    }

    // Возвращает складские действа (приходы, расходы) по подобию номера
    async findWarehouseActions(kind, pattern, partnerId = "") {
        const url = "./api/v1/yard/WarehouseActions?" + "kind=" + kind + "&pattern=" + pattern + "&partner_id=" + partnerId;
        return await this.getHttpResult("data", url);
    }

    // Возвращает складские действа (приходы, расходы), которые привязаны к указанной операции двора
    async getSelectedWarehouseActions(operationId) {
        const url = "./api/v1/yard/SelectedWarehouseActions?" + "operation_id=" + operationId;
        return await this.getHttpResult("data", url);
    }

    // Добавление складского действа (прихода, расхода) в операцию двора (OLD)
    async addOperationWarehouseAction(operationId, actionKind, actionId) {
        const url = "./api/v1/yard/AddOperationWarehouseAction";
        const data = {
            operationId: operationId,
            actionKind: actionKind,
            actionId: actionId
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("data", url, options);
    }

    // Удаление складского действа (прихода, расхода) из операции двора
    async removeOperationWarehouseAction(operationId, actionKind, actionId) {
        const url = "./api/v1/yard/RemoveOperationWarehouseAction";
        const data = {
            operationId: operationId,
            actionKind: actionKind,
            actionId: actionId
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("data", url, options);
    }

    // Удаление ВСЕХ складских действ (приходов, расходов) из операции двора
    async removeOperationWarehouseActions(operationId) {
        const url = "./api/v1/yard/RemoveOperationWarehouseActions";
        const data = {
            operationId: operationId
        };
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        return await this.getHttpResult("data", url, options);
    }
}

const api = new Api();