import { Table } from '../_Meta/Table';

export class DepartmentTable extends Table {
    static displayName = DepartmentTable.name;

    constructor(props) {
        super(props);
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
}
