import { Table } from '../_Meta/Table';
import { EmployeesCard } from '../Cards/EmployeesCard';

export class EmployeesTable extends Table {
    static displayName = EmployeesTable.name;

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
                <h1 id="tabelLabel" >Сотрудники</h1>
                <EmployeesCard selectedData={this.state.activeRowIndex >= 0 ? this.state.body[this.state.activeRowIndex] : {}} key={this.state.activeRowIndex} updateSetValCallback={this.updateSetValCallback} />
                {table}
            </div>
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Employee');
        const data = await response.json();
        console.log(data);

        super.setState({
            headers: {
                Surname: "Фамилия",
                Name: "Имя",
                Patronymic: "Отчество",
                Company: "Компания",
                Sex: "Пол",
                BirthDate: "Дата рождения",
            },
            dataTypes: {
                Surname: "string",
                Name: "string",
                Patronymic: "string",
                Company: "company",
                Sex: "string",
                BirthDate: "date",
            },
            body: data,
            selectRow: this.selectRowCallback,
            deleteRow: this.deleteRowCallback,
            activeRowIndex: 0,
        });
        this.selectRowCallback(-1, this);

    }
}
