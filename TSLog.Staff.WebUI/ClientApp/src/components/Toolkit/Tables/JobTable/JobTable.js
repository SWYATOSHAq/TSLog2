import React from 'react';
import { Table } from '../../_Meta/Table';
import { SearchPanel } from '../../_Meta/SearchPanel/SearchPanel';
import { JobsList} from "../../_Meta/JobsList/JobsList";
import { ItemStatusFilter } from "../../_Meta/ItemStatusFilter/ItemStatusFilter";
import {BlockLevelButton} from "../../_Meta/BlockLevelButton/BlockLevelButton";
import './JobTable.css'
import LoadInfo from "../../../../services/loadInfo";

export class JobTable extends Table{

    maxId = 100;



    loadItem = new LoadInfo();

    componentDidMount() {
        this.createItem();
    }

    // createItem() {
    //     this.loadItem.getProfessions().then((item) => {
    //         this.setState({
    //             jobListData: item
    //         });
    //     });


    createItem() {
        console.log('Загружается state', this.createJoblistObject());
        this.setState({
                jobListData: this.createJoblistObject()
            });

        // return {
        //     label:
        //     important: false,
        //     inSearch: false,
        //     id: this.maxId++
        // }
        console.log('State загружен')
    }

    state = {

        // jobListData: [
        //     this.createItem('Менеджер по продажам'),
        //     this.createItem('Кладовщик'),
        //     this.createItem('Водитель высотного штабелера'),
        //     this.createItem('Начальник склада')
        // ],
        jobListData: [],
        term: '',
        filter: 'все', // все, в штате, в подборе
    }

    createJoblistObject(){
        let arrOfObj = [];
        Promise.all([this.loadItem.getProfessions(), this.loadItem.getId()]).then(body => {
            for (let i = 0; i < body[0].length; i++) {
                arrOfObj.push({
                    label: body[0][i],
                    important: false,
                    inSearch: false,
                    id: body[1][i]
                });
            }
        });
        console.log(arrOfObj);
        return arrOfObj;
    }
        // {
        //     label: 'Менеджер по продажам',
        //     important: false,
        //     inSearch: false,
        //     id: 2001
        // },
        // {
        //     label: 'Завхоз',
        //     important: false,
        //     inSearch: false,
        //     id: 2002
        // }
        // this.loadItem.getProfessions().then(body => {
        //     for (let i = 0; i < body.length; i++) {
        //         arrOfObj.push({
        //             label: body[i],
        //             important: false,
        //             inSearch: false,
        //             id: this.maxId++
        //         });
        //     }
        //
        // });


    // createItem(label) {
    //     return {
    //         label,
    //         important: false,
    //         inSearch: false,
    //         id: this.maxId++
    //     }
    // }

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
        const newItem = this.createItem(text);
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

    toggleProperty(arr, id, propName) {
        const idx = arr.findIndex((el) => el.id === id);

        //1. Update obj
        const oldItem = arr[idx];
        const newItem = {...oldItem, [propName]: !oldItem[propName]};
        //2. Construct new array
        return [...arr.slice(0, idx),
            newItem,
            ...arr.slice(idx + 1)
        ];
    }

    onToggleImportant = (id) => {
        this.setState(({ jobListData }) => {
            return {
                jobListData: this.toggleProperty(jobListData, id, 'important')
            };
        });
    }
    onToggleInSearch = (id) => {
        this.setState(({ jobListData }) => {
            return {
                jobListData: this.toggleProperty(jobListData, id, 'inSearch')
            };
        });
    }

    search(items, term) {
        if (term.length === 0 ) {
            return items;
        }
        return items.filter((item) => {
            return item.label.toLowerCase().indexOf(term.toLowerCase()) > -1;
        });
    }

    onSearchChange = (term) => {
        this.setState({ term });
    }

    onFilterChange = (filter) => {
        this.setState({ filter });
    }

    filterItems(items, filter) {
        switch (filter) {
            case 'все':
                return items;
            case 'в подборе':
                return items.filter((item) => item.inSearch);
            case 'в штате':
                return items.filter((item) => !item.inSearch);
            default:
                return items;
        }
    }

    render(){
        const visibleItems = this.filterItems(this.search(this.state.jobListData, this.state.term), this.state.filter);
        console.log('Rendering', visibleItems);
        return (
            <div>
                <h1 id="tabelLabel" >Справочник профессий</h1>
                <div className='status-string'>
                    <SearchPanel
                        onSearchChange={ this.onSearchChange }/>
                    <ItemStatusFilter
                        filter={ this.state.filter }
                        onFilterChange={this.onFilterChange}/>
                </div>
                <BlockLevelButton onItemAdded={ this.addItem }/>
                <JobsList list={ visibleItems }
                    onDeleted={ this.deleteItem }
                    onToggleImportant={ this.onToggleImportant }
                    onToggleInSearch={ this.onToggleInSearch }/>
            </div>
        );
    }
}





