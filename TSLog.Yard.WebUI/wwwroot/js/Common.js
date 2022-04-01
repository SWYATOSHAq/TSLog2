class Utils {
    getUrlParameter(name) {
        const parameters = new URLSearchParams(location.search);
        const value = parameters.get(name);
        return value != null ? value : "";
    }

    getNowDate() {
        return (new Date()).toLocaleString("ru-RU", { year: 'numeric', month: 'numeric', day: 'numeric' });
    }

    convertTime(stringTime) {
        return new Date(stringTime).toLocaleTimeString();
    }

    convertDate(stringDate) {
        return new Date(stringDate).toLocaleDateString();
    }
}

const utils = new Utils();