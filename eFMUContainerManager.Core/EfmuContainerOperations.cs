﻿/*
 * Copyright (c) 2021, dSPACE GmbH, Modelica Association and contributors
 *
 * Licensed under the 3-Clause BSD license(the \"License\");    
 * you may not use this software except in compliance with    
 * the License.                                               
 *                                                            
 * This software is not fully developed or tested.            
 *                                                            
 * THE SOFTWARE IS PROVIDED \"as is\", in the hope that it may  
 * be useful to other users, without any warranty of any      
 * kind, either express or implied.                           
 *                                                            
 * See the License for the specific language governing        
 * permissions and limitations under the License.             
 */

namespace eFMI.ContainerManager
{
    public enum EfmuContainerOperations
    {
        UNKNOWN,
        CreateContainer,
        AddToContainer,
        ReplaceInContainer,
        DeleteFromContainer,
        ExtractFromContainer,
        ExtractSchemasFromContainer,
        UnpackFmu,
        TidyRoot,
        ListContainerContent
    }
}
