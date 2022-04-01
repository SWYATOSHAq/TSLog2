class GuardPage {
    shortUrl;

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    passVehicle(id, action) {
        if (action == "START_OPERATION") {
            api.startOperation(id)
        }
        else if (action == "FINISH_OPERATION") {
            api.finishOperation(id);
        }
    }

    letVehicleIn(id) {
        passVehicle(id, "START_OPERATION");
    }

    releaseVehicle(id) {
        passVehicle(id, "FINISH_OPERATION");
    }

    showGuardPage() {
        location.href = this.shortUrl;
    }
}

const guardPage = new GuardPage();