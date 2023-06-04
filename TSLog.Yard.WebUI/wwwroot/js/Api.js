/*
    Функционал для доступа к другим программным интерфейсам по http протоколу.
    Автор - Буров Михаил (m.burov@tslog.ru).
*/

// Класс для доступа к внешним программным интерфейсам по протоколу http
class Api {
    // Возвращает признак пользователя партнёра (клиента)
    async getIsPartnerUser() {
        let response = await fetch("./api/v1/yard/IsPartnerUser");
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("isPartnerUser", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает плановую продолжительность операции двора типу операции и виду автомашины (стандартное значение для ПОЛНОЙ машины)
    async getStandardPlanOperationDuration(operationTypeCode, vehicleKindCode) {
        const response = await fetch("./api/v1/yard/StandardPlanOperationDuration" +
            "?operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("planDuration", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("planDuration", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("planDuration", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает плановую продолжительность загрузки заказа (на отгрузку)
    async getHoodManPlanOutgoDuration(outgoId, isPallet, partnerId) {
        const response = await fetch("./api/v1/yard/HoodManPlanOutgoDuration" +
            "?outgo_id=" + outgoId +
            "&is_pallet=" + isPallet + 
            "&partner_id=" + partnerId);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("outgoDuration", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает плановые трудозатраты операции двора типу операции и виду автомашины 
    // (+ плановые трудозатраты только линейного персонала), стадартное значение плановых трудозатрат
    async getStandardPlanOperationLaborCost(operationTypeCode, vehicleKindCode) {
        let response = await fetch("./api/v1/yard/StandardPlanOperationLaborCost?" +
            "operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("laborCost", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает допустимые интервалы по дате, типу операции двора, виду автомашины
    async getAvalibleIntervals(date, operationTypeCode, vehicleKindCode) {
        let response = await fetch("./api/v1/yard/AvalibleIntervals?" +
            "date=" + date +
            "&operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("avalibleIntervals", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает свободные или зарезервированные (НЕ ЗАНЯТЫЕ) ворота по дате, типу операции двора, виду автомашины.
    async getAvalibleGates(startDate, finishDate, operationTypeCode, vehicleKindCode, operationId) {
        let response = await fetch("./api/v1/yard/AvalibleGates?" +            
            "operation_type=" + operationTypeCode +
            "&vehicle_kind=" + vehicleKindCode +
            "&start_date=" + startDate +
            "&finish_date=" + finishDate + 
            "&operation_id=" + operationId);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("avalibleGates", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает клиентского менеджера (основного)
    async getPartnerManager(partnerId) {
        let response = await fetch("./api/v1/yard/PartnerManager" +
            "?partner_id=" + partnerId);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("manager", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает операцию двора по идентификатору
    async getOperation(id) {
        let response = await fetch("./api/v1/yard/Operation?id=" + id);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массив операций двора по заданным параметрам поиска
    async getOperations(parameters) {
        let url = "./api/v1/yard/Operations" + parameters;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operations", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает активные (машины на территории складского комплекса) операции двора
    async getActiveOperations() {
        let url = "./api/v1/yard/ActiveOperations";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operations", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массив операций двора для охраны (которые должны приехать или могут уехать)
    async getGuardOperations() {
        let url = "./api/v1/yard/GuardOperations";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operations", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Создайт операцию двора и возвращает созданную (для сотрудника ТСЛ) без подтверждения
    async createOperation(data) {
        const url = "./api/v1/yard/CreateOperation";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Создайт операцию двора и возвращает созданную (для сотрудника ТСЛ) с подтверждением
    async createConfirmOperation(data) {
        const url = "./api/v1/yard/CreateConfirmOperation";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Создайт операцию двора и возвращает созданную (для сотрудника партнёра)
    async createOperationByPartner(data) {
        const url = "./api/v1/yard/CreateOperationByPartner";
        const options = {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Правит операцию двора и возвращает исправленную (для сотрудника ТСЛ)
    async editOperation(data) {
        const url = "./api/v1/yard/EditOperation";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Правит операцию двора и возвращает исправленную (для сотрудника партнёра)
    async editOperationByPartner(data) {
        const url = "./api/v1/yard/EditOperationByPartner";
        const options = {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Устанавливает ворота для операции двора
    async setOperationGate(id, gateCode) {
        const url = "./api/v1/yard/SetOperationGate";
        const data = {
            id: id
        }
        if (gateCode != "")
            data.gateCode = gateCode;
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };

        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // // Возвращает массив работников, которые могут быть назначены на данную должность, по шаблону поиска
    async getPositionWorkers(positionCode, pattern) {
        const url = "./api/v1/yard/PositionWorkers?" +
            "name_pattern=" + pattern +
            "&position_code=" + positionCode;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массов линейных сотрудников (всех)
    async getLinearWorkers() {
        const url = "./api/v1/yard/LinearWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массов линейных сотрудников (только свободных)
    async getLinearFreeWorkers() {
        const url = "./api/v1/yard/LinearFreeWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массов всех работников
    async getAllWorkers() {
        const url = "./api/v1/yard/AllWorkers";
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массив работников, назначенных на операцию двора
    async getOperationWorkers(operationId) {
        const url = "./api/v1/yard/OperationWorkers?operation_id=" + operationId;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Автоматически распределяет работников на операцию двора в зависимости от нормативов (потребностей в персонале)
    async allocateOperationWorkers(id) {
        const url = "./api/v1/yard/AllocateOperationWorkers";
        const data = {
            id: id
        }
        const options = {
            method: "PATCH",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        };
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает массив промежутков работ сотрудников из систем БоссКонтролль
    async getBossControlWorks(operationId) {
        const url = "./api/v1/yard/BossControlWorks?" +
            "operation_id=" + operationId
        let response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("works", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("operation", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует оперативный отчёт (за смену)
    async createOperationalReport(date, shift) {
        let url = "./api/v1/yard/OperationalReport?date=" + date + "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт диспетчера (за период)
    async createDispatcherReport(fromDate, toDate) {
        let url = "./api/v1/yard/DispatcherReport?from_date=" + fromDate + "&to_date=" + toDate;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт по трудозатратам
    async createLaborReport(fromDate, toDate) {
        let url = "./api/v1/yard/LaborReport?from_date=" + fromDate + "&to_date=" + toDate;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт по производительности за смену
    async createShiftPerfomanceReport(date, shift) {
        let url = "./api/v1/yard/ShiftPerfomanceReport?date=" + date + "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт по трудозатратам за смену 2
    async createShiftPerfomanceReport2(date, shift) {
        let url = "./api/v1/yard/ShiftPerfomanceReport?date=" + date + "&shift=" + shift;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт по трудозатратам за смену 3
    async createGateManagement(date) {
        let url = "./api/v1/yard/GateManagement?date=" + date;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт по выручке/расходам (БоссКоннтроль + Биллинг)
    async createECReport(fromDate, toDate, ids) {
        let url = "./api/v1/yard/ECReport?from_date=" + fromDate + "&to_date=" + toDate + "&partners_ids=" + ids;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует детализированный отчёт по трудозатратам персонала на конктретном клиенте (манданте) за выбранный период
    async createECReportDetailed(fromDate, toDate, id) {
        let url = "./api/v1/yard/ECReportDetailed?from_date=" + fromDate + "&to_date=" + toDate + "&partners_id=" + id;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт производительности по работникам (за период), первая версия
    async createWorkersPerfomanceReport(fromDate, toDate) {
        let url = "./api/v1/yard/WorkersPerfomanceReport?from_date=" + fromDate + "&to_date=" + toDate;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Формирует отчёт производительности по работникам (за период), вторая версия
    async createWorkersPerfomanceReport2(fromDate, toDate) {
        let url = "./api/v1/yard/WorkersPerfomanceReport2?from_date=" + fromDate + "&to_date=" + toDate;
        const response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает складские действа (приходы, расходы) по подобию номера
    async findWarehouseActions(kind, pattern, partnerId = "") {
        const url = "./api/v1/yard/WarehouseActions?" +
            "kind=" + kind +
            "&pattern=" + pattern + 
            "&partner_id=" + partnerId;
        let response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Возвращает складские действа (приходы, расходы), которые привязаны к указанной операции двора
    async getSelectedWarehouseActions(operationId) {
        const url = "./api/v1/yard/SelectedWarehouseActions?" +
            "operation_id=" + operationId;
        let response = await fetch(url);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }

    // Добавление складского действа (прихода, расхода) в операцию двора
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
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
        const response = await fetch(url, options);
        if (response.status >= 200 && response.status < 300) {
            return utils.createCorrectObject("data", await response.json());
        }
        else {
            return utils.createErrorObject(await response.text());
        }
    }
}

const api = new Api();