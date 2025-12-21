import React, { useEffect, useState } from 'react';
import { getProjectList } from '../ProjectList/ProjectListService';
import { PageContainer } from '../../../Basic/PageContainer';
import ProjectListAllGrid from './ProjectListAllGrid';
import { SetMaskOnOff } from '../../../Basic/SDOExtension';
import { CheckIsRDECRole } from '../../../Basic/CommonService';
import { GetBasicData } from '../../../Basic/BasicData';

export default function ProjectListAllMain() {
	// 計畫列表資料
	const [gridData, setGridData] = useState([]);
	// 是否具管考權限
	const [isRDEC, setIsRDEC] = useState(false);
	// 登入者機關
	const [orgId, setOrgId] = useState('');

	// 取得資料
	const loadData = async () => {
		await SetMaskOnOff(true);
		// 取得角色權限 & 機關
		const isRdec = await CheckIsRDECRole();
		setIsRDEC(isRdec);
		let orgId = await GetBasicData("orgId");
		setOrgId(orgId);
		// 取得計畫清單資料
		let result = await getProjectList({});
		if (result && result.length) {
			// let sortedProject = await sortProjectListData([...result]);
			setGridData([...result])
		}
		await SetMaskOnOff(false);
	}

	/**
	 * 排序計畫 依審核階段(升冪)、年度(降冪)、計畫編號(降冪)
	 * @param {Array} datas 
	 * @returns 
	 */
	const sortProjectListData = async (datas) => {
		// 排除排序目標為null
		let filteredData = datas.filter(x => x.PROJECT_STATUS_C != null && x.PROJECT_YEAR != null && x.PROJECT_NO != null)
		filteredData.sort(function (a, b) {
			if (a.PROJECT_STATUS_C === b.PROJECT_STATUS_C) {
				return a.PROJECT_NO < b.PROJECT_NO ? 1 : -1
			} else {
				return a.PROJECT_STATUS_C < b.PROJECT_STATUS_C ? -1 : 1;
			}
		});
		return filteredData;
	}

	/**
	 * 篩選填報、審查待辦清單
	 * @param {*} type 清單類別
	 */
	const filterDataByType = (type) => {
		switch (type) {
			case "add": // 計畫填報列表
				// 篩選作業階段為 => 1：立案中、3：立案退回、4：執行情形 、6：結案退回
				// 篩選主管機關或執行機關為登入者機關
				return gridData.filter(x => (x.PROJECT_STATUS_C === '1' || x.PROJECT_STATUS_C === '3' || x.PROJECT_STATUS_C === '4' || x.PROJECT_STATUS_C === '6') && x.EXEC_ORGAN_C === orgId);
			case "audit":  // 計畫審查列表
				// 篩選作業階段為 => 2：立案審核、5：結案審核 或 計畫調整狀態 => A02：調整審核(基本資料)、B02：調整審核(期程)、W02：撤銷審查
				return gridData.filter(x => x.PROJECT_STATUS_C === '2' || x.PROJECT_STATUS_C === '5' || x.PROJECT_AW_STATUS_C === "A02" || x.PROJECT_AW_STATUS_C === "B02" || x.PROJECT_AW_STATUS_C === "W02");
			default:
				return [];
		}
	}

	useEffect(() => {
		loadData();
	}, [])

	return (
		<PageContainer >
			{/* 計畫填報清單 */}
			<ProjectListAllGrid
				listType={'Add'}
				data={filterDataByType('add')}
				orgId={orgId}
			/>
			{/* 計畫審查清單 */}
			{/* 非管考權限不顯示 */}
			{isRDEC &&
				<>
					<br />
					<ProjectListAllGrid
						listType={'Audit'}
						data={filterDataByType('audit')}
					/>
				</>
			}
		</PageContainer>
	);
}
