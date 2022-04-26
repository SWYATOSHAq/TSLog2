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

    addParameter(parameters, parameterName, elementName) {
        const element = document.getElementById(elementName);
        if (element != undefined && element.value != "") {
            if (parameters != "") {
                parameters = parameters + "&";
            }
            parameters = parameters + parameterName + "=" + element.value;
        }
        return parameters;
    }
}

const utils = new Utils();