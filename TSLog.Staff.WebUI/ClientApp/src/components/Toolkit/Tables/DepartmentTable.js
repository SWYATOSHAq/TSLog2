import { Table } from '../_Meta/Table';

export class DepartmentTable extends Table {
    static displayName = DepartmentTable.name;

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
                <h1 id="tabelLabel" >подразделения</h1>
                {table}
            </div>
        );
    }

    async LoadData() {
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
