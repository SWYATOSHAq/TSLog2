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
        const response = await fetch('weatherforecast');
        const data = await response.json();
        console.log(data);

        super.setState({
            headers: { date: "Дата", summary: "Итог" , temperatureC: "Темп. С", temperatureF: "Темп. F"},
            body: data
        });

    }
}
