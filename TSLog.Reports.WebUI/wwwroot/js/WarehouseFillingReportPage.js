/*
  Страница отчёта по загруженности склада (#2198 WEB отчёт по загруженности склада. Первый приоритет.).
  Автор - Буров Михаил.
  Создано - 2022.09.08.
*/

// Класс страницы отчёта по загруженности склада
class WarehouseFillingReportPage {
    shortUrl;

    data = [];

    constructor() {
        this.shortUrl = location.href.split('?')[0];
    }

    createParameters() {
        let result = "";
        return result != "" ? "?" + result : "";
    }

    async getData() {

        const result = await api.getWarehouseFillingReportData();
        if (result.code === 0) {
            this.data = result.data;
        } else {
            viewUtils.showError(result.message);
        }
    }

    showBlock(divId, captionPercentDivId, valueField, color) {
        let fill = 0;
        let free = 0;

        const blockFillingDiv = document.getElementById(divId)
        fill = Math.round(this.data[valueField] * 100);
        free = 100 - fill;
        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + fill + "%; margin-top: " + free + "%";

        const captionPercentDiv = document.getElementById(captionPercentDivId)
        captionPercentDiv.textContent = fill + " %";
    }

    showReport() {
        this.showBlock("HBW-filling-div", "HBW-caption-percent-div", "HBW", "lightskyblue");
        this.showBlock("LBW-filling-div", "LBW-caption-percent-div", "LBW", "lightskyblue");
        this.showBlock("LB1-1-2-filling-div", "LB1-1-2-caption-percent-div", "LB1_12", "lightskyblue");
        this.showBlock("P01-filling-div", "P01-caption-percent-div", "P01", "lightskyblue");

        this.showBlock("VKU-filling-div", "VKU-caption-percent-div", "VKU", "bisque");
        this.showBlock("LM2-filling-div", "LM2-caption-percent-div", "LM2", "bisque");
        this.showBlock("LB1-3-E-filling-div", "LB1-3-E-caption-percent-div", "LB1_3E", "bisque");
        this.showBlock("P02-filling-div", "P02-caption-percent-div", "P02", "bisque");

        this.showBlock("LF3-filling-div", "LF3-caption-percent-div", "LF3", "lightpink");
        this.showBlock("P03-filling-div", "P03-caption-percent-div", "P03", "lightpink");
    }

    print() {
        window.print();
    }

    async show() {
        await this.getData();

        this.showReport();
        const url = this.shortUrl + this.createParameters();
        history.pushState({}, null, url);
    }
}

const warehouseFillingReportPage = new WarehouseFillingReportPage();

document.addEventListener('DOMContentLoaded', warehouseFillingReportPage.show.bind(warehouseFillingReportPage));