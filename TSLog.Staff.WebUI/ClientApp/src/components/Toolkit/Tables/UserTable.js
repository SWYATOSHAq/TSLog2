import { Table } from '../_Meta/Table';

export class UserTable extends Table {
    static displayName = UserTable.name;

    constructor(props) {
        super(props);
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
}
