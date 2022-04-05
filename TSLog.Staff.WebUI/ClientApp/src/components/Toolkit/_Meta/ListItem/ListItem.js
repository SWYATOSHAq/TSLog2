import React, { Component } from 'react';
import './ListItem.css'

export class ListItem extends Component {
    static displayName = ListItem.name;

    state = {
        inSearch: false,
        important: false
    }
    onLabelClick = () => {
        this.setState(({done: inSearch}) => {
            return {
                inSearch: !inSearch
            }
        })
    }
    onMarkImportant = () => {
        this.setState((state) => {
            return {
                important: !state.important
            }
        })
    }
    render() {
        const { inSearch, important } = this.state;
        let classNames = 'profession';
        if (inSearch) {
            classNames += ' inSearch';
        }
        if (important) {
            classNames += ' important';
        }
        return (
            <div className='change-button'>
                <span className= { classNames } onClick={ this.onLabelClick } >{this.props.label}</span>
                <div>
                    <button type="button" className="btn btn-outline-primary button-icons"
                    onClick={this.onMarkImportant}><i className="fa-solid fa-id-card-clip"></i></button>
                    <button type="button" className="btn btn-outline-danger button-icons"
                    onClick={this.props.onDeleted}><i className='fa fa-trash-o'/></button>
                </div>
            </div>
    );
    };
};
