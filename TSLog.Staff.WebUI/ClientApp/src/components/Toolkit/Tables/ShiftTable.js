import { Table } from '../_Meta/Table';
import { ShiftCard } from '../Cards/ShiftCard';

export class ShiftTable extends Table {
    static displayName = ShiftTable.name;

    constructor(props) {


        super(props);
        this.state = {};
    }

    updateSetValCallback = (childData) => {
        this.setState({ name: childData })
    }

    selectRowCallback = (e,index) => {
        alert("fuck");
        //this.setState(state => {
        //    const data = state.data;
        //    const selectedData = data[index];
        //    return ({
        //        data, selectedData
        //    });
        //});
    }

    componentDidMount() {
        this.LoadData();
    }

    render() {

        let table = super.render();

        return (
            <div>
                <h1 id="tabelLabel" >смены</h1>
                <h5>состояние активной записи таблицы: {JSON.stringify(this.state.selectedData)}</h5>
                <ShiftCard selectedData={this.state.selectedData} updateSetValCallback={this.updateSetValCallback}/>
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
            body: data,
            selectRow: this.selectRowCallback,
        });

        this.setState({
            selectedData: data[0],
            data: data,
        });

    }
}
