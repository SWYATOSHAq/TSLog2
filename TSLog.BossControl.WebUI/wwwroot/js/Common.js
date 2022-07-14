/*
    Общий функционал.
    Автор - Буров Михаил.
*/

// Класс, реализующий дополнительный и вспомогательный функционал
class Utils {
    // Получает значение параметра адресной строки по имени параметра
    getUrlParameter(name) {
        const parameters = new URLSearchParams(location.search);
        const value = parameters.get(name);
        return value != null ? value : "";
    }

    // Возвращает текущую дату и время
    getNowDate() {
        return (new Date()).toLocaleString("ru-RU", { year: 'numeric', month: 'numeric', day: 'numeric' });
    }

    // Нулевая дата
    nullDate = new Date(0);

    convertTime(stringTime) {
        return new Date(stringTime).toLocaleTimeString();
    }

    convertTimeHHMM(stringTime) {
        return new Date(stringTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    convertDate(stringDate) {
        return new Date(stringDate).toLocaleDateString();
    }

    // Преобразует дату в строку (только дату)
    dateToString(date) {
        return date.toLocaleDateString();
    }

    // Преобразует время в строку (только время)
    timeToString(date) {
        return date.toLocaleTimeString();
    }

    // Преобразует время в строку (только время) в формате HH:ММ
    timeToStringHHMM(date) {
        return new date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    }

    // Преобразует дату и время в строку
    dateTimeToString(date) {
        return date.toLocaleDateString() + " " + date.toLocaleTimeString();
    }

    secondsToString(seconds) {
        const hours = Math.trunc(seconds / 3600);
        const minutes = Math.trunc((seconds - hours * 3600) / 60);
        return hours.toString().padStart(2, "0") + ":" + minutes.toString().padStart(2, "0");
    }

    // Преобразует строку в дату
    stringToDate(stringDate) {
        var pattern = /(\d{2})\.(\d{2})\.(\d{4})/;
        return new Date(stringDate.replace(pattern, '$3-$2-$1'));
    }

    // Добавляет значение параметра к строке (значение берёт из элемента управления)
    addParameter(parameters, parameterName, elementName) {
        const element = document.getElementById(elementName);
        if (element != undefined && element.value != "") {
            parameters = this.addParameterValue(parameters, parameterName, element.value);
        }
        return parameters;
    }
    
    // Добавляет значение параметра к строке
    addParameterValue(parameters, parameterName, value) {
        if (parameters != "") {
            parameters = parameters + "&";
        }
        parameters = parameters + parameterName + "=" + value;
        return parameters;
    }

    // Создаёт объект, возвращаемый Api функциями в случае нормальной работы функции
    createCorrectObject(fieldName, data, message = "Ok", code = 0) {
        return {
            code: code,
            message: message,
            [fieldName]: data
        }
    }

    // Создаёт объект, возвращаемый Api функциями в случае ошибки
    createErrorObject(message, code = 9999) {
        return {
            code: code,
            message: message
        }
    }

    // Отображает корректно дробное число (с округлением, с разделением разрядов)
    displayFloat(value, decimalPoints = 0) {
        const w = value.toFixed(decimalPoints);
        const k = w | 0;
        const b = value < 0 ? 1 : 0;
        const u = Math.abs(w - k);
        const d = ("" + u.toFixed(decimalPoints)).substr(2, decimalPoints);
        const s = "" + k;
        let i = s.length;
        let r = "";
        while ((i -= 3) > b) {
            r = " " + s.substr(i, 3) + r;
        }
        return s.substr(0, i + 3) + r + (d ? "," + d : "");
    };

    filterData(data, filters) {
        let resultData = [];
        for (let element of data) {
            let isValid = true;
            for (let filter of filters) {
                if (filter.condition === "=") {
                    if (element[filter.fieldName] != filter.value) {
                        isValid = false;
                        break;
                    }
                } else if (filter.condition === "like") {
                    if (!(element[filter.fieldName].toLowerCase().replace("ё", "е").includes(filter.value))) {
                        isValid = false;
                        break;
                    }
                } else if (filter.condition === "!=") {
                    if (element[filter.fieldName] === filter.value) {
                        isValid = false;
                        break;
                    }
                } else if (filter.condition === "<") {
                    if (element[filter.fieldName] >= filter.value) {
                        isValid = false;
                        break;
                    }
                } else if (filter.condition === ">") {
                    if (element[filter.fieldName] <= filter.value) {
                        isValid = false;
                        break;
                    }
                }
            }
            if (isValid) {
                resultData.push(element);
            }
        }
        return resultData;
    }
}

const utils = new Utils();