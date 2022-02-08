import { Table } from '../_Meta/Table';

export class EmployeesTable extends Table {
    static displayName = EmployeesTable.name;

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
                <h1 id="tabelLabel" >Сотрудники</h1>
                {table}
            </div>
        );
    }

    async LoadData() {
        const response = await fetch('/api/v1/Staff/Employee');
        const data = await response.json();
        console.log(data);

        super.setState({
            headers: { FullName: "ФИО", Sex: "Должность", BirthDate: "Дата рождения", Code: "Отдел" },
            dataTypes: { FullName: "string", Sex: "string", BirthDate: "date", Sex: "string" },
            body: data
        });

    }
}
