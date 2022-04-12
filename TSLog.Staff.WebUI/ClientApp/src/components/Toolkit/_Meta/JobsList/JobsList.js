import React, { Component } from 'react';
import { ListItem} from "../ListItem/ListItem";

export class JobsList extends Component {
    static displayName = JobsList.name;

    render() {
        const elements = this.props.list.map((item) => {
            const { id, ...itemProps} = item;

            const getResource = async(url) => {
                const res = await fetch(url);
                if (!res.ok) {
                    throw new Error(`Could not fetch ${url}` +
                        `, received ${res.status}`)
                }
                const body = await res.json();
                return body;
            };

            getResource('http://staff.apsk.ru/api/v1/Staff/Position')
                .then((body) => {
                    console.log(body[0].Code);
                })
                .catch((err) => {
                    console.error('Could not fetch',err);
                });


            return (
                <li key={id} className="list-group-item">
                    <ListItem {...itemProps}
                    onDeleted={() => this.props.onDeleted(id)}
                    onToggleImportant={() => this.props.onToggleImportant(id)}
                    onToggleInSearch={() => this.props.onToggleInSearch(id)}/>
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