/*
    Общий функционал.
    Автор - Буров Михаил.
*/

class Utils {
    getUrlParameter(name) {
        const parameters = new URLSearchParams(location.search);
        const value = parameters.get(name);
        return value != null ? value : "";
    }

    getNowDate() {
        return (new Date()).toLocaleString("ru-RU", { year: 'numeric', month: 'numeric', day: 'numeric' });
    }

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

    dateToString(date) {
        return date.toLocaleDateString();
    }

    timeToString(date) {
        return date.toLocaleTimeString();
    }

    dateTimeToString(date) {
        return date.toLocaleDateString() + " " + date.toLocaleTimeString();
    }

    secondsToString(seconds) {
        const hours = Math.trunc(seconds / 3600);
        const minutes = Math.trunc((seconds - hours * 3600) / 60);
        return hours.toString().padStart(2, "0") + ":" + minutes.toString().padStart(2, "0");
    }

    stringToDate(stringDate) {
        var pattern = /(\d{2})\.(\d{2})\.(\d{4})/;
        return new Date(stringDate.replace(pattern, '$3-$2-$1'));
    }

    addParameter(parameters, parameterName, elementName) {
        const element = document.getElementById(elementName);
        if (element != undefined && element.value != "") {
            parameters = this.addParameterValue(parameters, parameterName, element.value);
        }
        return parameters;
    }

    addParameterValue(parameters, parameterName, value) {
        if (parameters != "") {
            parameters = parameters + "&";
        }
        parameters = parameters + parameterName + "=" + value;
        return parameters;
    }
    
    createCorrectObject(fieldName, data, message = "Ok", code = 0) {
        return {
            code: code,
            message: message,
            [fieldName]: data
        }
    }

    createErrorObject(message, code = 9999) {
        return {
            code: code,
            message: message
        }
    }
}

const utils = new Utils();