import React from 'react';
import { Table } from '../_Meta/Table';
import { SearchPanel } from '../_Meta/SearchPanel';
import { JobsList} from "../_Meta/JobsList";

export class JobTable extends Table{

    render(){
        return (
            <div>
                <h1 id="tabelLabel" >Справочник профессий</h1>
                <SearchPanel />
                <JobsList list={JobListData}/>
            </div>
        );
    }
}


const JobListData = [
    { label: 'Менеджер по продажам', important: false },
    { label: 'Кладовщик', important: true },
    { label: 'Водитель высотного штабелера', important: false }
];



