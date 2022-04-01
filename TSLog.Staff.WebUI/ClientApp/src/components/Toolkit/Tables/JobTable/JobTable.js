import React from 'react';
import { Table } from '../../_Meta/Table';
import { SearchPanel } from '../../_Meta/SearchPanel/SearchPanel';
import { JobsList} from "../../_Meta/JobsList/JobsList";
import { ItemStatusFilter } from "../../_Meta/ItemStatusFilter/ItemStatusFilter";
import {BlockLevelButton} from "../../_Meta/BlockLevelButton/BlockLevelButton";
import './JobTable.css'

export class JobTable extends Table{

    render(){
        return (
            <div>
                <h1 id="tabelLabel" >Справочник профессий</h1>
                <div className='status-string'>
                    <SearchPanel />
                    <ItemStatusFilter />
                </div>
                <BlockLevelButton />
                <JobsList list={JobListData}/>
            </div>
        );
    }
}


const JobListData = [
    { label: 'Менеджер по продажам', important: false, id: 1 },
    { label: 'Кладовщик', important: true, id: 2 },
    { label: 'Водитель высотного штабелера', important: false, id: 3 }
];



