import React from 'react';
import { Table } from '../../_Meta/Table';
import { SearchPanel } from '../../_Meta/SearchPanel/SearchPanel';
import { JobsList} from "../../_Meta/JobsList/JobsList";
import { ItemStatusFilter } from "../../_Meta/ItemStatusFilter/ItemStatusFilter";
import {BlockLevelButton} from "../../_Meta/BlockLevelButton/BlockLevelButton";
import './JobTable.css'

export class JobTable extends Table{

    maxId = 100;

    state = {
        jobListData: [
            { label: 'Менеджер по продажам', important: false, id: 1 },
            { label: 'Кладовщик', important: true, id: 2 },
            { label: 'Водитель высотного штабелера', important: false, id: 3 }
        ]
    }
    deleteItem = (id) => {
        this.setState(({ jobListData }) => {
            const idx = jobListData.findIndex((el) => el.id === id);

            const newArray = [...jobListData.slice(0, idx),
                ...jobListData.slice(idx + 1)];

            return {
                jobListData: newArray
            };
        });
    }

    addItem = (text) => {
        const newItem = {
            label: text,
            important: false,
            id: this.maxId++
        }
        this.setState(({ jobListData }) => {
            const newArr = [
                ...jobListData,
                newItem
            ];
            return {
                jobListData: newArr
            };
        })
    }

    render(){
        return (
            <div>
                <h1 id="tabelLabel" >Справочник профессий</h1>
                <div className='status-string'>
                    <SearchPanel />
                    <ItemStatusFilter />
                </div>
                <BlockLevelButton onItemAdded={ this.addItem }/>
                <JobsList list={this.state.jobListData}
                onDeleted={ this.deleteItem }/>
            </div>
        );
    }
}


// const JobListData = [
//     { label: 'Менеджер по продажам', important: false, id: 1 },
//     { label: 'Кладовщик', important: true, id: 2 },
//     { label: 'Водитель высотного штабелера', important: false, id: 3 }
// ];



