using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem.Interactions;
using PV_Enums;
using UnityEditor;

namespace PV_Utils
{
    /// <summary>Utilities class</summary>
    public static class PV_Utilities
    {
        #region OBJECT AND COMPONENT HANDLING-----------------------//////////////////////////////////
        /// <summary>Returns a list of the specified components from objects with the passed tag.</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tag_passed"></param>
        /// <returns></returns>
        public static List<T> GetComponentsWithTag_inScene<T>( string tag_passed)
        {
            //DbgMethod($"GetComponentsWithTag('{tag_passed}')");

            GameObject[] _temp = GameObject.FindGameObjectsWithTag(tag_passed);
            List<T> list_return = new List<T>();
            for (int i = 0; i < _temp.Length; i++)
            {
                list_return.Add(_temp[i].GetComponent<T>());

            }
            return list_return;
        }

        /// <summary>
        /// Gets components in ONLY children, excluding the parent from the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent_passed"></param>
        /// <returns></returns>
		public static List<T> GetComponentsInOnlyChildren<T>( GameObject parent_passed ) where T: Component
		{
			List<T> components = new List<T>( parent_passed.GetComponentsInChildren<T>() );

            T parentComponent;
            
            if( parent_passed.TryGetComponent<T>(out parentComponent) )
            {
                if( components.Contains(parentComponent) )
                {
					components.Remove(parentComponent);
                }
            }
 
			return components;
		}

		/// <summary>Returns a list of components with the option to exclude certain passed components. This allows you to exclude the parent's component from the list, for example.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parent_passed"></param>
		/// <param name="components_toExcludeAlso"></param>
		/// <returns></returns>
		public static List<T> GetComponentsInChildren_selective<T>(GameObject parent_passed, params T[] components_toExcludeAlso) where T : Component
        {
            //Transform[] transforms_all = t_passed.GetComponentsInChildren<Transform>();
            List<T> components = new List<T>( parent_passed.GetComponentsInChildren<T>() );

            if ( components_toExcludeAlso != null && components_toExcludeAlso.Length > 0 )
            {
                foreach ( T t in components_toExcludeAlso )
                {
                    if ( components.Contains(t) )
                    {
                        components.Remove(t);
                    }
                }
            }
            return components;
        }

        public static bool HasOneOfTheseTags( GameObject go, params string[] tags )
        {
            foreach( string tagString in tags )
            {
                if( go.CompareTag(tagString) )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Instantiates an object, while preserving the connection to the prefab in the assets folder, keeping it blue in the heirarchy.
        /// </summary>
        /// <param name="prefabRef"></param>
        /// <param name="parentTrans"></param>
        /// <param name="orientTrans"></param>
        /// <returns></returns>
        public static GameObject ConnectedInstantiate( GameObject prefabRef, Transform parentTrans, Transform orientTrans )
        {
			GameObject go = PrefabUtility.InstantiatePrefab(prefabRef) as GameObject;
            go.transform.parent = parentTrans;
            go.transform.position = orientTrans.position;
            go.transform.rotation = orientTrans.rotation;

            return go;
		}

		#endregion

		#region VECTORS/RECTS --------------//////////////////////////////////
		/// <summary>
		/// Returns the supplied vector with a y of 0. 
		/// </summary>
		/// <param name="v_passed"></param>
		/// <param name="makeNormalized">Leave true to return a normalized vector, set false to return the vector at its original length.</param>
		/// <returns></returns>
		public static Vector3 FlatVect( Vector3 v_passed, bool makeNormalized = true )
        {
            if( makeNormalized )
                return new Vector3(v_passed.x, 0f, v_passed.z).normalized;
            else
                return new Vector3(v_passed.x, 0f, v_passed.z);

        }
        /// <summary>
        /// Returns the supplied vector with a y of 0 and placed directly behind the supplied transform.. 
        /// </summary>
        /// <param name="v_passed"></param>
        /// <param name="makeNormalized">Leave true to return a normalized vector, set false to return the vector at its original length.</param>
        /// <returns></returns>
        public static Vector3 FlatVect( Vector3 v_passed, Transform transform_passed, bool makeNormalized = true )
        {
            if (makeNormalized)
                return new Vector3(v_passed.x, 0f, v_passed.z).normalized;
            else
                return new Vector3(v_passed.x, 0f, v_passed.z);

        }

        public static Vector3 SkinnyVect( Vector3 v_passed, bool nrmlzd = true )
        {
            Vector3 v_return = Vector3.zero;

            //if (v_passed.z > 0)
            //    v_return = Quaternion.FromToRotation(flatVect(v_passed), (v_passed.z > 0 ? Vector3.forward : Vector3.back)) * v_passed;

            v_return = new Vector3(0f, v_passed.y, v_passed.z);

            if (nrmlzd)
                v_return = v_return.normalized;

            return v_return;
        }

        /// <summary>
        /// Describes how directly or indirectly the supplied transform is facing the supplied position.  
        /// </summary>
        /// <param name="transform_passed"></param>
        /// <param name="position_passed"></param>
        /// <returns>Float from -1 to 1 that describes how directly in front of (positive), or directly behind (negative), the supplied position is when compared to the supplied transform's forward vector. 
        /// This describes a simplified, single-number representation of the InverseTransformPoint function. The ITP function's x value (peripheral relative positioning) will effect the magnitude of the decimal point value, 
        /// but a positive value will always be relatively in front and a negative value will always be relatively behind.</returns>
        public static float FlatFacingPerspective( Transform transform_passed, Vector3 position_passed )
        {
            Vector3 v = transform_passed.InverseTransformPoint(position_passed).normalized;

            float val = 1 - Mathf.Abs(v.x);
            if( v.z < 0f )
            {
                val *= -1;
            }


            return val;
        }
        /// <summary>
        /// Describes how directly or indirectly the supplied transform is facing the supplied position.  
        /// </summary>
        /// <param name="transform_passed"></param>
        /// <param name="position_passed"></param>
        /// <returns>Float from -1 to 1 that describes how directly in front of (positive), or directly behind (negative), the supplied position is when compared to the supplied transform's forward vector. 
        /// This describes a simplified, single-number representation of the InverseTransformPoint function. The ITP function's x value (peripheral relative positioning) will effect the magnitude of the decimal point value, 
        /// but a positive value will always be relatively in front and a negative value will always be relatively behind.</returns>
        public static float FlatFacingPerspective(Transform transform_passed, Vector3 position_passed, out Vector3 vITP_passed )
        {
            vITP_passed = transform_passed.InverseTransformPoint(position_passed).normalized;

            float val = 1 - Mathf.Abs(vITP_passed.x);
            if ( vITP_passed.z < 0f )
            {
                val *= -1;
            }

            return val;
        }

        public static float GetArea( Vector2 v )
        {
            return v.x * v.y;
        }
		public static float GetArea( RectTransform rt)
		{
			return rt.rect.size.x * rt.rect.size.y;
		}
		#endregion



		#region ARRAY UTILITIES----------------------------/////////////////////////////////////////

		public static GameObject[] assignAll( int arraySize )
        {
            GameObject[] gObArray = new GameObject[arraySize];
            for( int i = 0; i < arraySize; i++ )
            {
                gObArray[i] = null;
            }

            return gObArray;
        }

        /// <summary>
        /// Returns a 'looped' index, meaning if the index goes above the passed list count, or below 0, it will loop the index while
        /// staying within the list bounds.
        /// </summary>
        /// <param name="listCount_passed">count property of the list you're currently intending to cycle through</param>
        /// <param name="index_passed">Your intended index. If the index goes above the count of the list that you pass, or below 0,
        /// it will use this index as a staring point to determine the cycled index.</param>
        /// <returns></returns>
        public static int GetLoopedIndex( int listCount_passed, int index_passed )
        {
            PV_Debug.Log( $"GetLoopedIndex(listCount_passed: '{listCount_passed}', index_passed: '{index_passed}')", PV_LogFormatting.UserMethod );
            //TODO: Check for if the passed index is multiple times larger (or smaller, IE: negatives) than the passed list's count...

            if( index_passed >= listCount_passed)
            {
                PV_Debug.Log($"returning: '{index_passed - listCount_passed}'", PV_LogFormatting.Standard );
                return index_passed - listCount_passed;
            }
            else if( index_passed < 0 )
            {
                PV_Debug.Log($"returning: '{listCount_passed - Mathf.Abs(index_passed)}'");
                return listCount_passed - Mathf.Abs(index_passed);
            }
            else
            {
                PV_Debug.Log($"returning: '{index_passed}'", PV_LogFormatting.Standard );
                return index_passed;
            }
        }
        #endregion

        //-----------------[[ N U M E R I C A L ]]----------------\\


        #region PHYSICS -----------------------//////////////////////////////////
        /// <summary>
        /// Casts multiple times in a cross formation around origin. If any of the casts finds a hit, it stops the operation immediately and returns true.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="radius"></param>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static bool CrossCast( Vector3 origin, float radius, out RaycastHit hitInfo, int layerMask, QueryTriggerInteraction queryTriggerInteraction )
        {
            bool dbg = false;
            if (dbg) Debug.Log($"CrossCast() '{origin}'");
            if( Physics.Linecast(origin + (Vector3.up * radius), origin + (Vector3.down * radius), out hitInfo, layerMask, queryTriggerInteraction) )
            {
                if (dbg) Debug.Log($"linecast found hit at '{hitInfo.point}'");
                return true;
            }
            else if (Physics.Linecast(origin + (Vector3.down * radius), origin + (Vector3.up * radius), out hitInfo, layerMask, queryTriggerInteraction))
            {
                if (dbg) Debug.Log($"linecast found hit at '{hitInfo.point}'");
                return true;
            }

            if (Physics.Linecast(origin + (Vector3.right * radius), origin + (Vector3.left * radius), out hitInfo, layerMask, queryTriggerInteraction))
            {
                if (dbg) Debug.Log($"linecast found hit at '{hitInfo.point}'");
                return true;
            }
            else if (Physics.Linecast(origin + (Vector3.left * radius), origin + (Vector3.right * radius), out hitInfo, layerMask, queryTriggerInteraction))
            {
                if (dbg) Debug.Log($"linecast found hit at '{hitInfo.point}'");
                return true;
            }

            if (Physics.Linecast(origin + (Vector3.forward * radius), origin + (Vector3.back * radius), out hitInfo, layerMask, queryTriggerInteraction))
            {
                if (dbg) Debug.Log($"linecast found hit at '{hitInfo.point}'");
                return true;
            }
            else if (Physics.Linecast(origin + (Vector3.back * radius), origin + (Vector3.forward * radius), out hitInfo, layerMask, queryTriggerInteraction))
            {
                if (dbg) Debug.Log($"linecast found hit at '{hitInfo.point}'");
                return true;
            }
            return false;
        }
        #endregion

        public static bool IsInsideVolume(Vector3 pos_passed, Bounds bounds_passed )
        {
            if ( pos_passed.z > bounds_passed.min.z && pos_passed.z < bounds_passed.max.z && 
                pos_passed.x > bounds_passed.min.x && pos_passed.x < bounds_passed.max.x && 
                pos_passed.y > bounds_passed.min.y && pos_passed.y < bounds_passed.max.y )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

		#region UI HELPERS --------------////////////////////////////////
        public static Vector2 GridLayoutGroupRectIsBigEnoughForCells( GridLayoutGroup glg, Button[] btns )
        {
            RectTransform rt_glg = glg.GetComponent<RectTransform>();
            
            if( rt_glg.rect.size.x < (glg.cellSize.x + glg.spacing.x) || rt_glg.rect.size.y < (glg.cellSize.y + glg.spacing.y) )
            {
                return Vector2.zero;
            }

            Debug.Log( $"size: '{rt_glg.rect.size}', cellsize: '{glg.cellSize}'" );
            int containedCollumns = (int)( rt_glg.rect.size.x / (glg.cellSize.x + glg.spacing.x) );
			float remainingSpace = rt_glg.rect.size.x - ( containedCollumns * (glg.cellSize.x + glg.spacing.x) );

			Debug.Log( $"initially calculated: '{containedCollumns}' columns. remaining space will be: '{remainingSpace}'" );
            if( remainingSpace >= glg.cellSize.x )
            {
				Debug.Log("adding to collumns");
				containedCollumns++;
            }

			int containedRows = (int)( rt_glg.rect.size.y / (glg.cellSize.y + glg.spacing.y) );

            remainingSpace = rt_glg.rect.size.y - ( containedRows * (glg.cellSize.y + glg.spacing.y) );
			Debug.Log($"initially calculated: '{containedRows}' rows. remaining space will be: '{remainingSpace}'");
			if ( remainingSpace >= glg.cellSize.y )
			{
				Debug.Log("adding to rows");
				containedRows++;
			}

            Debug.Log( $"contained rows: '{containedRows}', cntndcollums: '{containedCollumns}'. total: '{containedRows * containedCollumns}'" );
            if( (containedCollumns * containedRows) >= btns.Length )
            {
			    return new Vector2( containedCollumns, containedRows );

            }
            else
            {
                return Vector2.zero;
            }
        }

        public static void SetNavigationOnGridLayoutGroupButtons( GridLayoutGroup glg, Button[] btns )
        {
			RectTransform rt_glg = glg.GetComponent<RectTransform>();
            Vector2 v_gridCellCounts = GridLayoutGroupRectIsBigEnoughForCells( glg, btns );
            if( v_gridCellCounts == Vector2.zero )
            {
				PV_Debug.LogError($"grid layout group rect is not big enough for current cell amount. Returning early...");
				return;
			}

			int containedCollumns = (int)v_gridCellCounts.x;
			int containedRows = (int)v_gridCellCounts.y;
            int totalCellsAvailable = containedCollumns * containedRows;

			for ( int i = 0; i < btns.Length; i++ )
			{
				Navigation nav = btns[i].navigation; //for some retarded reason, I have to create a navigation variable and assign it to the button rather than using the "selectOn..." properties of the button, otherwise it will throw error...
				int indx = -1;
				
                //up
                if( i >= containedCollumns )
                {
                    indx = i - containedCollumns;
                }
                else
                {
                    indx = totalCellsAvailable - containedCollumns + i;

                    if( indx >= btns.Length )
                    {
                        indx = totalCellsAvailable - containedCollumns - containedCollumns + i;
                    }
                } //todo: bug gets thrown pointing to this line (so probably something inside this block) when there's only one row
                nav.selectOnUp = btns[indx];

                //down
                if( i < totalCellsAvailable - containedCollumns )
                {
                    indx = i + containedCollumns;
                }
                else
                {
                    indx = i - (totalCellsAvailable - containedCollumns);
                }
                nav.selectOnDown = btns[indx];

                //right
                if( (i+1) % containedCollumns == 0 )
                {
                    //Debug.Log( $"looping for right at: '{i}'" );
                    indx = i - containedCollumns + 1;
                }
                else
                {
                    indx = i + 1;
                }
                nav.selectOnRight = btns[indx];

                if( i % containedCollumns == 0 )
                {
					Debug.Log( $"looping for left at: '{i}'" );
					indx = i + containedCollumns - 1;
				}
                else
                {
                    indx = i - 1;
                }
                nav.selectOnLeft = btns[indx];

                btns[i].navigation = nav;
				UnityEditor.EditorUtility.SetDirty( btns[i] ); // Apparently I have to do this for the change to take. When I didn't have this in, the change wouldn't persist into play mode.
			}
		}
		#endregion
	}
}
