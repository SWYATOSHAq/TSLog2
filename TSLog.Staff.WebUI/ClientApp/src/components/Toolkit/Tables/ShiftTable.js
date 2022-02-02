import { Table } from '../_Meta/Table';

export class ShiftTable extends Table {
    static displayName = ShiftTable.name;

    constructor(props) {


        super(props);

    }

    componentDidMount() {
        this.LoadData();
    }

    render() {

        let table = super.render();

        return (
            <div>
                <h1 id="tabelLabel" >смены</h1>
                {table}
            </div>
        );
    }

    async LoadData() {
        //const response = await fetch('weatherforecast');
        //const data = await response.json();
        //console.log(data);

        //super.setState({
        //    headers: { date: "Дата", summary: "Итог" , temperatureC: "Темп. С", temperatureF: "Темп. F"},
        //    dataTypes: { date: "datetime", summary: "string", temperatureC: "int", temperatureF: "int" },
        //    body: data
        //});

        const response = await fetch('/api/v1/Staff/Shifts');
        const data = await response.json();
        console.log(data);

        super.setState({
            headers: { WorkMode: "Тип работы", From: "От", To: "До" },
            dataTypes: { WorkMode: "string", From: "datetime", To: "datetime" },
            body: data
        });

    }
}
