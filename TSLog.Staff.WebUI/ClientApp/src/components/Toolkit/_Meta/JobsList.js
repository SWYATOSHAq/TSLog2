import React, { Component } from 'react';
import { ListItem} from "./ListItem";

export class JobsList extends Component {
    static displayName = JobsList.name;

    render() {
        return (
            <ul>
                <li>< ListItem
                    label= {this.props.list[0].label}
                    important= {this.props.list[0].important} /></li>
                <li>< ListItem
                    label = {this.props.list[1].label}
                    important= {this.props.list[1].important} /></li>
                 <li>< ListItem
                    label = {this.props.list[2].label}
                    important= {this.props.list[2].important} /></li>
            </ul>
        );
    }
}