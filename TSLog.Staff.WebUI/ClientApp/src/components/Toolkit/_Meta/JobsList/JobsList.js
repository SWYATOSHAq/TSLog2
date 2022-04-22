import React, { Component } from 'react';
import { ListItem } from "../ListItem/ListItem";

export class JobsList extends Component {
    static displayName = JobsList.name;


    render() {
    //     const elements = this.props.list.map(() => {
    //         const { id, ...itemProps} = item;
    //         return (
    //             <li key={id} className="list-group-item">
    //                 <ListItem {...itemProps}
    //                 onDeleted={() => this.props.onDeleted(id)}
    //                 onToggleImportant={() => this.props.onToggleImportant(id)}
    //                 onToggleInSearch={() => this.props.onToggleInSearch(id)} />
    //             </li>
    //         );
    //     });
        const elements = this.props.list.map((item) => {
            const { id, ...itemProps} = item;
            console.log('Отображается список');
            return (
                <li key={id} className="list-group-item">
                    <ListItem {...itemProps}
                    onDeleted={() => this.props.onDeleted(id)}
                    onToggleImportant={() => this.props.onToggleImportant(id)}
                    onToggleInSearch={() => this.props.onToggleInSearch(id)} />
                </li>
            );
            });
        return (
            <ul className="list-group">
                { elements }
            </ul>
        );
    }
}