import React, { Component } from 'react';
import './ListItem.css'

export class ListItem extends Component {
    static displayName = ListItem.name;

    render() {

        let classNames = 'profession';
        if (this.props.inSearch) {
            classNames += ' inSearch';
        }
        if (this.props.important) {
            classNames += ' important';
        }
        return (
            <div className='change-button'>
                <span className= { classNames } onClick={ this.props.onToggleInSearch } >{this.props.label}</span>
                <div>
                    <button type="button" className="btn btn-outline-primary button-icons"
                    onClick={this.props.onToggleImportant}><i className="fa-solid fa-id-card-clip"></i></button>
                    <button type="button" className="btn btn-outline-danger button-icons"
                    onClick={this.props.onDeleted}><i className='fa fa-trash-o'/></button>
                </div>
            </div>
    );
    };
};
