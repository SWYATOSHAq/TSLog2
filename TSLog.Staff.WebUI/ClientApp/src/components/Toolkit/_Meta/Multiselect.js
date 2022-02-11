import React, { Component } from "react";
import "../../../scss/Arrow.scss";

export class Multiselect extends Component {
    static displayName = Multiselect.name;

    constructor(props) {
        super(props);
        this.state = {
            storedItems: [
                { DisplayName: "aaa" },
                { DisplayName: "aaa" },
                { DisplayName: "aaa" },
            ],
            inputItems: [
                { DisplayName: "fffff1" },
                { DisplayName: "fffff2" },
                { DisplayName: "ffff3" },
                { DisplayName: "ffff4" },
                { DisplayName: "ffff5" },
                { DisplayName: "ffff6" },
                { DisplayName: "ffff7" },
                { DisplayName: "ffff8" },
                { DisplayName: "ffff9" },
                { DisplayName: "ffff10" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
                { DisplayName: "fffff" },
            ],
            value:[""],
        };
    }

    handleChange(event) {
        let newVal = event.target.value;
        //let stateVal = this.state.value

        //console.log(stateVal)
        console.log(newVal);

        console.log(event);

        //stateVal.indexOf(newVal) === -1
        //    ? stateVal.push(newVal)
        //    : stateVal.length === 1
        //        ? (stateVal = [])
        //        : stateVal.splice(stateVal.indexOf(newVal), 1)

        //this.setState({ value: stateVal })
    }

    render() {
        return (
            <div className="p-2 row">
                <div className="p-2 col-4">
                    <h5>Выбранные</h5>
                    <select className="form-select" multiple>
                        {
                            this.state.storedItems.map((row, i) => {
                                return (<option key={i} value={i}>{row.DisplayName}</option>)
                            })
                        }
                      </select>
                </div>
                <div className="col-4">
                    <div class="arrow-container">
                        <button class="arrow left"></button>
                        <button class="arrow right"></button>
                    </div>
                </div>
                <div className="p-2 col-4">
                    <h5>Все</h5>
                    <select
                        className="form-select"
                        multiple
                        value={this.state.value}
                        onChange={this.handleChange}
                    >
                        {
                            this.state.inputItems.map((row, i) => {
                                return (<option key={i} value={i}>{row.DisplayName} {"<"}-{i}  </option>)
                            })
                        }
                      </select>
                </div>

                <div className="p-2 col-12">
                    {JSON.stringify(this.state.inputItems)}
                </div>
            </div>

        );
    }
}