function GetUrlParameter(name) {
    const parameters = new URLSearchParams(window.location.search);
    var value = parameters.get(name);
    return value != null ? value : "";
}