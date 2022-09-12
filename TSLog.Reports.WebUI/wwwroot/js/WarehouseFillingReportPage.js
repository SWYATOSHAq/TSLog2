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

    showBlock(area, color) {
        let fill = 0;
        let free = 0;

        const blockFillingDiv = document.getElementById(area + "-filling-div")
        fill = Math.round(this.data[area + "_percent"] * 100);
        free = 100 - fill;
        blockFillingDiv.style.cssText = "background-color: " + color + "; height: " + fill + "%; margin-top: " + free + "%";

        const captionPercentDiv = document.getElementById(area + "-caption-percent-div");
        captionPercentDiv.textContent = fill + " %";

        const captionValueDiv = document.getElementById(area + "-caption-value-div")
        captionValueDiv.textContent = "";
    }

    showReport() {
        this.showBlock("HBW", "lightskyblue");
        this.showBlock("LBW", "lightskyblue");
        this.showBlock("LB1_1_2", "lightskyblue");
        this.showBlock("P01", "lightskyblue");

        this.showBlock("VKU", "bisque");
        this.showBlock("LM2", "bisque");
        this.showBlock("LB1_3_E", "bisque");
        this.showBlock("P02", "bisque");

        this.showBlock("LF3", "lightpink");
        this.showBlock("P03", "lightpink");
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