import React, { Component } from 'react';
import './ListItem.css'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faIdCardClip } from '@fortawesome/free-solid-svg-icons'
import { faTrashCan } from '@fortawesome/free-regular-svg-icons'

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
                    onClick={this.props.onToggleImportant}><FontAwesomeIcon icon= { faIdCardClip } /></button>
                    <button type="button" className="btn btn-outline-danger button-icons"
                    onClick={this.props.onDeleted}><FontAwesomeIcon icon= { faTrashCan } /></button>
                </div>
            </div>
    );
    };
};
