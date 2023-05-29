import { Table } from '../_Meta/Table';
import { EmployeesCard } from '../Cards/EmployeesCard';
import { CreateNewButton } from '../_Meta/CreateNewButton';

export class EmployeesTable extends Table {
    static displayName = EmployeesTable.name;

    updateSetValCallback = (childData) => {
        console.log('-----------------------------------');
        console.log(childData);
        console.log(JSON.stringify(childData));
        console.log('-----------------------------------');
        //this.setState({ name: childData });

        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(childData)
        };

        fetch('/api/v1/Staff/Employee', requestOptions)
            .then(response => response.json())
            .then(newData => {
                console.log(newData);
                console.log("УУУУУУРРРРРРРРРРАААААААААА")                    ;
                //this.setState(state => {
                //    const headers = state.headers;
                //    const dataTypes = state.dataTypes;
                //    const activeRowIndex = state.activeRowIndex;
                //    const body = state.body;
                //    body[activeRowIndex] = newData;

                //    const selectRow = state.selectRow;
                //    const deleteRow = state.deleteRow;

                //    return ({
                //        headers, dataTypes, body, selectRow, deleteRow, activeRowIndex,
                //    });
                //});
            })
            .catch((e) => {
                console.error(e.message); // "oh, no!"
            })        

    }

    selectRowCallback = (index, e) => {
        window.scrollTo(0, 0);
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
    createNewCallback = () => {
        this.setState(state => {
            const headers = state.headers;
            const dataTypes = state.dataTypes;
            const body = state.body;

            body.unshift({ Company: { DisplayName: "" } }); // pre append empty object

            const selectRow = state.selectRow;
            const deleteRow = state.deleteRow;

            return ({
                headers, dataTypes, body, selectRow, deleteRow, activeRowIndex: 0,
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
                <CreateNewButton CreateNew={this.createNewCallback} />
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
                FirstName: "Имя",
                Patronymic: "Отчество",
                Company: "Компания",
                Sex: "Пол",
                BirthDate: "Дата рождения",
            },
            dataTypes: {
                Surname: "string",
                FirstName: "string",
                Patronymic: "string",
                Company: "company",
                Sex: "gender",
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
