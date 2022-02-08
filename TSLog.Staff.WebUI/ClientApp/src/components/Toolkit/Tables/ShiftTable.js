import { Table } from '../_Meta/Table';
import { ShiftCard } from '../Cards/ShiftCard';

export class ShiftTable extends Table {
    static displayName = ShiftTable.name;

    constructor(props) {


        super(props);
        //this.state = {};
    }

    updateSetValCallback = (childData) => {
        this.setState({ name: childData })
    }

    selectRowCallback = (index, e) => {
        console.log(index);
        console.log(this.state.activeRowIndex);
        console.log(this.state.body[this.state.activeRowIndex]);
        this.setState(state => {
            const headers = state.headers;
            const dataTypes = state.dataTypes;
            const body = state.body;
            const selectRow = state.selectRow;

            return ({
                headers, dataTypes, body, selectRow, activeRowIndex: index,
            });
        });
    }

    componentDidMount() {
        this.LoadData();
    }

    render() {

        let table = super.render();

        return (
            <div>
                <h1 id="tabelLabel" >смены</h1>
                <h5>состояние активной записи таблицы: {JSON.stringify(this.state.body[this.state.activeRowIndex])}</h5>
                <ShiftCard selectedData={this.state.activeRowIndex >= 0 ? this.state.body[this.state.activeRowIndex] : {}} key={this.state.activeRowIndex} updateSetValCallback={this.updateSetValCallback} />
                {table}
            </div>
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Shift');
        const data = await response.json();
        //console.log(data);
        this.setState({
            headers: { WorkMode: "Тип работы", From: "От", To: "До" },
            dataTypes: { WorkMode: "string", From: "datetime", To: "datetime" },
            body: data,
            selectRow: this.selectRowCallback,
            activeRowIndex: 0,
        });

    }
}
