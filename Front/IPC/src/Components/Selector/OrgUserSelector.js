/*
    組織樹連動使用者列表 元件
*/
import React, { useState, useEffect, useReducer } from 'react';
import DualListBox from 'react-dual-listbox';
import Orgselector from './Orgselector'
import { ServerConfig } from '../../Basic/BasicData';
import { api } from '../../Basic/ApiFetch';

const OrgUserSelector = (props) => {
    /* use State */
    const [selectedEmpOrg, setSelectedEmpOrg] = useState([]);
    const [empUsers, setEmpUsers] = useState([]);
    /* use Effect */
    useEffect(() => {
        setEmpUsers(props.users);
    }, [props.users])
    useEffect(() => {
        //更新userList
        loadUsersByEmpOrg();
    }, [selectedEmpOrg])

    /* 雙向選單更改觸發 */
    const onListChangeHandler = event => {
        props.setValue(event);
    }

    /* 更新userList */
    const loadUsersByEmpOrg = async () => {
        // 保留已選擇users
        let users = empUsers.filter((user) => {
            // if user.USER_ID in value
            return props.value.includes(user.USER_ID)
        });
        try {
            for (let org of selectedEmpOrg) {
                let response = await api.Get(ServerConfig.backEndUrl + 'EmpOrg/GetOrgUsers/' + org.ORG_ID);
                var responseData = response.ok ? await response.json() : null;
                // 合併兩個array and移除duplicate item
                users = [...users, ...responseData].filter((user, index, array) => {
                    /* user exists, and 
                       current index = index of the first user with the same data */
                    return user && index === array.findIndex(e => e.USER_ID === user.USER_ID);
                })
            }
            setEmpUsers(users);
        }
        catch (e) {
            console.log('ErrorMessage: ' + e);
        }
    }

    return (
        <div style={{ width: '100%' }}>
            <Orgselector
                multiple={false}
                //當前組織選擇資料
                value={selectedEmpOrg}
                //設定資料call back
                setValue={(data) => { setSelectedEmpOrg(data) }} />

            <DualListBox
                //候選清單
                options={empUsers.map(item => {
                    return { label: item.USER_ID + '-' + item.USER_NAME, value: item.USER_ID };
                })}
                //已選擇清單
                selected={props.value}
                onChange={onListChangeHandler} />
        </div>
    )
}

export default OrgUserSelector;