import { Table } from '../_Meta/Table';
import { DepartmentCard } from '../Cards/DepartmentCard';

export class DepartmentTable extends Table {
    static displayName = DepartmentTable.name;

    constructor(props) {
        super(props);
    }



    updateSetValCallback = (childData) => {
        this.setState({ name: childData })
    }

    selectRowCallback = (index, e) => {
        this.setState(state => {
            const headers = state.headers;
            const dataTypes = state.dataTypes;
            const body = state.body;
            const selectRow = state.selectRow;
            const deleteRow = state.deleteRow;

            return ({
                headers, dataTypes, body, selectRow, deleteRow, activeRowIndex: index,
            });
        });
    }

    deleteRowCallback = (index, e) => {
        console.log(index);
        console.log(this.state.activeRowIndex);
        console.log(this.state.body[this.state.activeRowIndex]);
        this.setState(state => {
            const headers = state.headers;
            const dataTypes = state.dataTypes;
            const body = state.body;

            body.splice(index, 1); // remove 1 element by index

            const selectRow = state.selectRow;
            const deleteRow = state.deleteRow;
            const activeRowIndex = state.activeRowIndex;

            return ({
                headers, dataTypes, body, selectRow, deleteRow, activeRowIndex,
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
                <h1 id="tabelLabel" >подразделения</h1>
                <DepartmentCard selectedData={this.state.activeRowIndex >= 0 ? this.state.body[this.state.activeRowIndex] : {}} key={this.state.activeRowIndex} updateSetValCallback={this.updateSetValCallback} />
                {table}
            </div>
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Department');
        const data = await response.json();
        console.log(data);

        super.setState({
            headers: { DisplayName: "Наименование" },
            dataTypes: { DisplayName: "string" },
            body: data,
            selectRow: this.selectRowCallback,
            deleteRow: this.deleteRowCallback,
            activeRowIndex: 0,
        });
        this.selectRowCallback(-1, this);

    }
}
