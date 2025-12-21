using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SDO.Models;
using SDO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SDO.Dac
{
    public class SCFunctionDac : Dac,  ISetFunctionDac
    {
        public SCFunctionDac(IConnectionControlCenter connectionControlCenter,
            IHttpContextAccessor httpContextAccessor,
            ISqlTrace trace,
            IUserProfile profile,
            IParameterAdaptor ParameterAdaptor,
            IConfiguration configuration) : base(connectionControlCenter, httpContextAccessor, trace, profile, ParameterAdaptor, configuration)
        {
        }



        public async Task<IList<SetFunctionModel>> ReadByUser(string apId, string userId,string CompId="GSS")
        {
            string sql = @" SELECT	DISTINCT 
		                        D.FUN_ITEM_ID FUNCTION_ID ,
		                        D.FUN_ITEM_NAME FUNCTION_NAME ,
		                        D.FUN_ITEM_DESC ,
		                        D.PARENT_FUN_ITEM_ID PARENT_ID ,
		                        D.PRG_PATH FUNCTION_URL ,
		                        CAST(D.FUN_SORT_ORDER AS INT) AS SORT_ID,
		                        D.DISPLAY_TYPE ,
		                        D.ICON AS ICON_Class,
		                        CASE WHEN IsNull(Mark.USR_ID ,'')='' THEN 0 ELSE 1 END AS IsBookMarked
                        FROM GPREL_GRP_MBRM	A (NOLOCK)
                        JOIN SCREL_ROL_RGTM B (NOLOCK)
	                        ON	A.GRP_DOMAIN_ID = B.ROL_DOMAIN_ID
		                        AND A.GRP_COMP_ID = B.ROL_COMP_ID
		                        AND A.GRP_GROUP_ID = B.ROL_ID
                        JOIN SCREL_RGT_FUNITEMM C (NOLOCK)
	                        ON	B.RGT_AP_ID = C.RGT_AP_ID
		                        AND B.RGT_ID = C.RGT_ID
                        JOIN SCFUNITEMM D (NOLOCK)
	                        ON	C.FUN_ITEM_AP_ID = D.AP_ID
		                        AND C.FUN_ITEM_ID = D.FUN_ITEM_ID
                        LEFT JOIN SCBOOKMARKM Mark
	                        ON	Mark.USR_ID = A.MBR_MEMBER_ID 
                                AND Mark.FUN_ITEM_ID = D.FUN_ITEM_ID
                        WHERE	A.MBR_MEMBER_KIND = 'SC_USR'
		                        AND A.REL_KIND = '1'
		                        AND A.MBR_COMP_ID = @MBR_COMP_ID
		                        AND A.MBR_MEMBER_ID = @MBR_MEMBER_ID
		                        AND A.GRP_GROUP_KIND = N'SC_ROL'
		                        AND D.AP_ID = @AP_ID
		                        AND D.DISPLAY_TYPE = 'Y'
		                        AND LOWER(B.ROL_ID) IN
		                        (
			                        SELECT LOWER(A.ROL_ID)
                                    FROM SCREL_ROL_USRMV A (NOLOCK)
                                    JOIN SCREL_ROL_RGTM B (NOLOCK) 
				                        ON A.ROL_ID = B.ROL_ID AND RGT_AP_ID = @AP_ID
                                    WHERE USR_ID=@MBR_MEMBER_ID
                                    AND RGT_ID<>'APADMIN'
		                        )
                        ORDER BY CAST(D.FUN_SORT_ORDER AS INT)";

            return await ExecuteQueryAsync<SetFunctionModel>(
                sql, new
                {
                    MBR_COMP_ID = CompId,
                    MBR_MEMBER_ID = userId,
                    AP_ID = apId
                },SCDBKey);
        }
 
    }
}
