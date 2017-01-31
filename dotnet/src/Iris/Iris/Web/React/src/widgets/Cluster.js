import * as React from "react";
import Panel from 'muicss/lib/react/panel';
import { showModal } from '../App';
import { removeMember } from "iris";
import ADD_NODE from "../modals/AddNode";
import { map } from "../Util.ts";

export default class WidgetCluster extends React.Component {
  static get layout() {
    return { x: 0, y: 0, w: 12, h: 12, minW: 7, maxW: 15, minH: 11, maxH: 15 };
  }

  constructor(props) {
    super(props);
  }

  render() {
    return (
      <Panel className="panel-cluster">
        <table className="mui-table mui-table--bordered">
          <thead>
            <tr>
              <th>Host</th>
              <th>IP</th>
              <th>Port</th>
              <th>State</th>
              <th>Role</th>
              <th>Tags</th>
            </tr>
          </thead>
          <tbody>
            {map(this.props.info.state.Project.Config.Cluster.Members, kv => {
              const node = kv[1];
              return (
                <tr key={kv[0].Fields[0]}>
                  <td>{node.HostName}</td>
                  <td>{node.IpAddr.Fields[0]}</td>
                  <td>{node.Port}</td>
                  <td>{node.State.ToString()}</td>
                  <td>left</td>
                  <td>Main, VideoPB, Show1</td>
                  <td><a onClick={() => removeMember(this.props.info, kv[0])}>Remove</a></td>
                </tr>
              );
            })}
          </tbody>
        </table>
        <a onClick={() => showModal(ADD_NODE)}>Add node</a>
      </Panel>
    )
  }
}
